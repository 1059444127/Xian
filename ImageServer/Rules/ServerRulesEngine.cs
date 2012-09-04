#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using ClearCanvas.Common;
using ClearCanvas.Common.Actions;
using ClearCanvas.Common.Specifications;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom.Utilities.Rules;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;

namespace ClearCanvas.ImageServer.Rules
{
    /// <summary>
    /// Rules engine for applying rules against DICOM files and performing actions.
    /// </summary>
    /// <remarks>
    /// The ServerRulesEngine encapsulates code to apply rules against DICOM file 
    /// objects.  It will load the rules from the persistent store, maintain them by type,
    /// and then can apply them against specific files.
    /// </remarks>
    /// <seealso cref="ServerActionContext"/>
    /// <example>
    /// Here is an example rule for routing all images with Modality set to CT to an AE
    /// Title CLEARCANVAS.
    /// <code>
    /// <rule id="CT Rule">
    ///   <condition expressionLanguage="dicom">
    ///     <equal test="$Modality" refValue="CT"/>
    ///   </condition>
    ///   <action>
    ///     <auto-route device="CLEARCANVAS"/>
    ///   </action>
    /// </rule>
    /// </code>
    /// </example>
    public class ServerRulesEngine : RulesEngine<ServerActionContext,ServerRuleTypeEnum>
    {
        private readonly ServerRuleApplyTimeEnum _applyTime;
        private readonly ServerEntityKey _serverPartitionKey;

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// A rules engine will only load rules that apply at a specific time.  The
        /// apply time is specified by the <paramref name="applyTime"/> parameter.
        /// </remarks>
        /// <param name="applyTime">An enumerated value as to when the rules shall apply.</param>
        /// <param name="serverPartitionKey">The Server Partition the rules engine applies to.</param>
        public ServerRulesEngine(ServerRuleApplyTimeEnum applyTime, ServerEntityKey serverPartitionKey)
        {
            _applyTime = applyTime;
            _serverPartitionKey = serverPartitionKey;
            Statistics = new RulesEngineStatistics(applyTime.Lookup, applyTime.LongDescription);
        }

		#endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="ServerRuleApplyTimeEnum"/> for the rules engine.
        /// </summary>
        public ServerRuleApplyTimeEnum RuleApplyTime
        {
            get { return _applyTime; }
        }

        #endregion

        #region Public Methods

	
        /// <summary>
        /// Load the rules engine from the Persistent Store and compile the conditions and actions.
        /// </summary>
        public void Load()
        {
            Statistics.LoadTime.Start();

            // Clearout the current type list.
            _typeList.Clear();

            using (IReadContext read = PersistentStoreRegistry.GetDefaultStore().OpenReadContext())
            {
                IServerRuleEntityBroker broker = read.GetBroker<IServerRuleEntityBroker>();

                ServerRuleSelectCriteria criteria = new ServerRuleSelectCriteria();
                criteria.Enabled.EqualTo(true);
                criteria.ServerRuleApplyTimeEnum.EqualTo(_applyTime);
                criteria.ServerPartitionKey.EqualTo(_serverPartitionKey);

				// Add ommitted or included rule types, as appropriate
				if (_omitList.Count > 0)
					criteria.ServerRuleTypeEnum.NotIn(_omitList.ToArray());
				else if (_includeList.Count > 0)
					criteria.ServerRuleTypeEnum.In(_includeList.ToArray());

            	IList<ServerRule> list = broker.Find(criteria);

                // Create the specification and action compilers
                // We'll compile the rules right away
                XmlSpecificationCompiler specCompiler = new XmlSpecificationCompiler("dicom");
                XmlActionCompiler<ServerActionContext, ServerRuleTypeEnum> actionCompiler = new XmlActionCompiler<ServerActionContext, ServerRuleTypeEnum>();

                foreach (ServerRule serverRule in list)
                {
                    try
                    {
                        Rule<ServerActionContext, ServerRuleTypeEnum> theRule = new Rule<ServerActionContext, ServerRuleTypeEnum>();
                        theRule.Name = serverRule.RuleName;
                    	theRule.IsDefault = serverRule.DefaultRule;
                    	theRule.IsExempt = serverRule.ExemptRule;
                        theRule.Description = serverRule.ServerRuleApplyTimeEnum.Description;

                        XmlNode ruleNode =
                            CollectionUtils.SelectFirst<XmlNode>(serverRule.RuleXml.ChildNodes,
                                                                 delegate(XmlNode child) { return child.Name.Equals("rule"); });


						theRule.Compile(ruleNode, serverRule.ServerRuleTypeEnum, specCompiler, actionCompiler);

                        RuleTypeCollection<ServerActionContext, ServerRuleTypeEnum> typeCollection;

                        if (!_typeList.ContainsKey(serverRule.ServerRuleTypeEnum))
                        {
                            typeCollection = new RuleTypeCollection<ServerActionContext, ServerRuleTypeEnum>(serverRule.ServerRuleTypeEnum);
                            _typeList.Add(serverRule.ServerRuleTypeEnum, typeCollection);
                        }
                        else
                        {
                            typeCollection = _typeList[serverRule.ServerRuleTypeEnum];
                        }

                        typeCollection.AddRule(theRule);
                    }
                    catch (Exception e)
                    {
                        // something wrong with the rule...
                        Platform.Log(LogLevel.Warn, e, "Unable to add rule {0} to the engine. It will be skipped",
                                     serverRule.RuleName);
                    }
                }
            }

            Statistics.LoadTime.End();
        }

        #endregion
    }
}
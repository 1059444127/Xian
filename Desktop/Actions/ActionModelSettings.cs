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
using System.Configuration;
using System.Diagnostics;
using System.Xml;
using ClearCanvas.Common;
using ClearCanvas.Common.Configuration;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Desktop.Actions
{
	/// <summary>
    /// Provides services for storing an action model to an XML document, and rebuilding that action model from the document.
    /// </summary>
	[SettingsGroupDescription("Stores the action model document that controls ordering and naming of menus and toolbar items.")]
	[SettingsProvider(typeof(StandardSettingsProvider))]
	[UserSettingsMigrationDisabled]
	[SharedSettingsMigrationDisabled]
	internal sealed partial class ActionModelSettings : IDisposable
    {
		private XmlDocument _actionModelXmlDoc;

		private bool _temporary;


		private ActionModelSettings()
		{
			ApplicationSettingsRegistry.Instance.RegisterInstance(this);
		}

		public override void Upgrade()
		{
		}

		#region Public Methods

		/// <summary>
        /// Builds an in-memory action model from the specified XML model and the specified set of actions.
        /// </summary>
        /// <remarks>
        /// The actions will be ordered according to the XML model.  Any actions that are not a part of the
        /// XML model will be added to the memory model and inserted into the XML model based on a 'group hint'.
		/// The XML model is automatically persisted, and new models that have never before been persisted
		/// will be added.
		/// </remarks>
        /// <param name="namespace">A namespace to qualify the site.</param>
        /// <param name="site">The site.</param>
        /// <param name="actions">The set of actions to include. This set should be prefiltered on <paramref name="site"/>.</param>
        /// <returns>An <see cref="ActionModelNode"/> representing the root of the action model.</returns>
        public ActionModelRoot BuildAndSynchronize(string @namespace, string site, IActionSet actions)
        {
			// do one time initialization
			if(_actionModelXmlDoc == null)
				Initialize();

			string actionModelID = string.Format("{0}:{1}", @namespace, site);

			IDictionary<string, IAction> actionMap = BuildActionMap(actions);

			XmlElement xmlActionModel = Synchronize(actionModelID, actionMap);
			ActionModelRoot modelRoot = Build(site, xmlActionModel, actionMap);

			return modelRoot;
		}

		/// <summary>
		/// Builds an in-memory abstract action model from the specified XML model and the specified set of known actions.
		/// </summary>
		/// <remarks>
		/// This method functions similarly to <see cref="BuildAndSynchronize"/> except that the resulting action model
		/// consists solely of <see cref="AbstractAction"/>s which are not actually associated with any concrete actions on tools or components.
		/// </remarks>
		/// <param name="namespace">A namespace to qualify the site.</param>
		/// <param name="site">The site.</param>
		/// <param name="actions">The set of actions to include. This set should be prefiltered on <paramref name="site"/>.</param>
		/// <returns>An <see cref="ActionModelNode"/> representing the root of the action model.</returns>
		public ActionModelRoot BuildAbstractActionModel(string @namespace, string site, IActionSet actions)
		{
			// do one time initialization
			if (_actionModelXmlDoc == null)
				Initialize();

			string actionModelId = string.Format("{0}:{1}", @namespace, site);

			IDictionary<string, IAction> actionMap = BuildActionMap(actions);

			XmlElement xmlActionModel = FindXmlActionModel(actionModelId);
			if (xmlActionModel == null)
			{
				xmlActionModel = Synchronize(actionModelId, actionMap);
			}
			else
			{
				// clone the model because we don't want to be modifying the actual action model yet
				xmlActionModel = (XmlElement) xmlActionModel.CloneNode(true);

				// if there are new persistent actions that aren't already in the xml, insert them now
				foreach (IAction action in actionMap.Values)
				{
					if (action.Persistent)
					{
						if (AppendActionToXmlModel(_actionModelXmlDoc, xmlActionModel, action))
							Platform.Log(LogLevel.Debug, "Inserted {0}", action.ActionID);
					}
				}

				List<XmlElement> childNodes = GetActionNodeList(xmlActionModel);
				List<IAction> abstractActions = new List<IAction>(childNodes.Count);
				foreach (XmlElement childElement in childNodes)
				{
					if (childElement.Name == "action")
					{
						string actionId = childElement.GetAttribute("id");
						if (string.IsNullOrEmpty(actionId))
						{
							Platform.Log(LogLevel.Debug, "Invalid action model entry with null ID in /action-models/action-model[@id='{0}']", actionModelId);
							continue;
						}

						try
						{
							if (actionMap.ContainsKey(actionId))
							{
								abstractActions.Add(AbstractAction.Create(actionMap[actionId]));
							}
						}
						catch (Exception ex)
						{
							Platform.Log(LogLevel.Debug, ex, "Invalid action model entry at /action-models/action-model[@id='{0}']/action[@id='{1}']", actionModelId, actionId);
						}
					}
				}
				actions = new ActionSet(abstractActions);
				actionMap = BuildActionMap(actions);
			}

			return Build(site, xmlActionModel, actionMap);
		}

		/// <summary>
		/// Persists an in-memory abstract action model to the XML model.
		/// </summary>
		/// <remarks>
		/// This method functions as a counterpart to <see cref="BuildAbstractActionModel"/>. The specified abstract action model
		/// (created by <see cref="BuildAbstractActionModel"/>, or potentially modified further) is flattened and its nodes
		/// written out to the XML mode, replacing any existing model at the same qualified site. This allows action model
		/// configuration interfaces to make changes to the action model.
		/// </remarks>
		/// <param name="namespace">A namespace to qualify the site.</param>
		/// <param name="site">The site.</param>
		/// <param name="abstractActionModel">The abstract action model to be persisted.</param>
		/// <returns>An <see cref="ActionModelNode"/> representing the root of the action model.</returns>
		public void PersistAbstractActionModel(string @namespace, string site, ActionModelRoot abstractActionModel)
		{
			// do one time initialization
			if (_actionModelXmlDoc == null)
				Initialize();

			XmlElement separatorTemplate = _actionModelXmlDoc.CreateElement("separator");

			string actionModelId = string.Format("{0}:{1}", @namespace, site);

			XmlElement xmlActionModel = FindXmlActionModel(actionModelId);
			if (xmlActionModel != null)
			{
				// clear the action model
				List<XmlNode> childrenToClear = CollectionUtils.Cast<XmlNode>(xmlActionModel.ChildNodes);
				foreach (XmlNode childNode in childrenToClear)
					xmlActionModel.RemoveChild(childNode);
			}
			else
			{
				// add a new action model
				this.GetActionModelsNode().AppendChild(xmlActionModel = CreateXmlActionModel(actionModelId));
			}

			ActionModelNode[] leafNodes = abstractActionModel.GetLeafNodesInOrder();
			for (int n = 0; n < leafNodes.Length; n++)
			{
				ActionModelNode leafNode = leafNodes[n];
				if (leafNode is ActionNode)
				{
					xmlActionModel.AppendChild(CreateXmlAction(_actionModelXmlDoc, ((ActionNode) leafNode).Action));
				}
				else if (leafNode is SeparatorNode)
				{
					xmlActionModel.AppendChild(separatorTemplate.Clone());
				}
			}

			if (!_temporary)
			{
				this.ActionModelsXml = _actionModelXmlDoc;
				this.Save();
			}
		}

		public void Export(XmlWriter writer)
		{
			// do one time initialization
			if (_actionModelXmlDoc == null)
				Initialize();

			_actionModelXmlDoc.Save(writer);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			// if this was not a 'temporary' model, save it
			if (_actionModelXmlDoc != null && !_temporary)
			{
				try
				{
					this.ActionModelsXml = _actionModelXmlDoc;

					//Ticket #1551: temporarily disabled this until there is a UI for editing the action model (JR)
					//this.Save();
				}
				catch (Exception e)
				{
					// don't treat this as a serious error
					// not much we can do but log it
					Platform.Log(LogLevel.Error, e);
				}

				_actionModelXmlDoc = null;
			}

			// unregister from the registry
			ApplicationSettingsRegistry.Instance.UnregisterInstance(this);
		}

		#endregion

		#region Overrides

		protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			// in the event settings are re-loaded, need to clear the cached document
			if (e.PropertyName == "ActionModelsXml")
			{
				_actionModelXmlDoc = null;
			}

			base.OnPropertyChanged(sender, e);
		}

		#endregion

		#region Private Methods

		private void Initialize()
		{
			if(_actionModelXmlDoc == null)
			{
				try
				{
					// load the document from the store
					_actionModelXmlDoc = this.ActionModelsXml;
				}
				catch (Exception e)
				{
					// if this fails to load for some reason, don't treat it as a serious error
					// instead, just create a temporary XML document so the application can run
					Platform.Log(LogLevel.Error, e);
					_actionModelXmlDoc = new XmlDocument();
					_actionModelXmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<action-models />");

					// set 'temporary' flag to true, so we don't try and save this model to the store
					_temporary = true;
				}
			}
		}


		/// <summary>
		/// Builds a map of action IDs to actions.
		/// </summary>
		/// <param name="actions">the set of actions from which to build a map</param>
		/// <returns>a map of action IDs to actions</returns>
		private static IDictionary<string, IAction> BuildActionMap(IActionSet actions)
		{
			Dictionary<string, IAction> actionMap = new Dictionary<string, IAction>();

			foreach (IAction action in actions)
				actionMap[action.ActionID] = action;

			return actionMap;
		}

		/// <summary>
		/// Creates the specified action model, but *does not* immediately append it to the xmlDoc.
		/// Since not all actions are persistent (e.g. some could be generated), we need to figure
		/// out how many actions (if any) belonging to the node will be persisted in the store
		/// before adding the action to the store.
		/// </summary>
		/// <param name="id">the id of the "action-model" to create</param>
		/// <returns>An "action-model" element</returns>
		private XmlElement CreateXmlActionModel(string id)
		{
			XmlElement xmlActionModel = _actionModelXmlDoc.CreateElement("action-model");
			xmlActionModel.SetAttribute("id", id);
			return xmlActionModel;
		}

		/// <summary>
		/// Creates an "action" node for insertion into an "action-model" node in the Xml store.
		/// </summary>
		/// <param name="action">the action whose relevant properties are to be used to create the node</param>
		/// <returns>an "action" element</returns>
		private static XmlElement CreateXmlAction(XmlDocument document, IAction action)
		{
			XmlElement xmlAction = document.CreateElement("action");

			xmlAction.SetAttribute("id", action.ActionID);
			xmlAction.SetAttribute("path", action.Path.ToString());
			xmlAction.SetAttribute("group-hint", action.GroupHint.Hint);

			if (!action.Available)
				xmlAction.SetAttribute("available", action.Available.ToString());

			if (action is IClickAction)
			{
				IClickAction clickAction = (IClickAction) action;
				if (clickAction.KeyStroke != XKeys.None)
					xmlAction.SetAttribute("keystroke", XKeysConverter.FormatInvariant(clickAction.KeyStroke));
			}
			
			return xmlAction;
		}

		/// <summary>
		/// Finds a stored model in the XML doc with the specified model ID.
		/// </summary>
		/// <param name="id">The model ID</param>
		/// <returns>An "action-model" element, or null if not found</returns>
		private XmlElement FindXmlActionModel(string id)
		{
			return (XmlElement)this.GetActionModelsNode().SelectSingleNode(String.Format("/action-models/action-model[@id='{0}']", id));
		}

		/// <summary>
		/// Finds an action with the specified id in the specified "action-model" node.
		/// </summary>
		/// <param name="id">the id of the action to find</param>
		/// <param name="xmlActionModel">the "action-model" node to search in</param>
		/// <returns>the XmlElement of the action if found, otherwise null</returns>
		private static XmlElement FindXmlAction(string id, XmlElement xmlActionModel)
		{
			return (XmlElement)xmlActionModel.SelectSingleNode(String.Format("action[@id='{0}']", id));
		}

		/// <summary>
		/// Synchronizes persistent actions with the xml store.
		/// Refer to <see cref="BuildAndSynchronize"/> for more details.
		/// </summary>
		/// <param name="actionModelID">the ID of the action model</param>
		/// <param name="actionMap">the actions that are to be synchronized/added to the store</param>
		/// <returns>the "action-model" node with the specified actionModelID</returns>
		private XmlElement Synchronize(string actionModelID, IDictionary<string, IAction> actionMap)
		{
			bool changed = false;

			XmlElement xmlActionModel = FindXmlActionModel(actionModelID);
			bool modelExists = (xmlActionModel != null);
			if (!modelExists)
				xmlActionModel = CreateXmlActionModel(actionModelID);

			if (ValidateXmlActionModel(xmlActionModel, actionMap))
				changed = true;

			//make sure every action has a pre-determined spot in the store, inserting
			//actions appropriately based on their 'group hint'.  The algorithm guarantees 
			//that each action will get put somewhere in the store.  Only persistent actions
			//are added to the xml store; otherwise, the non-persistent actions would 
			//be determining the positions of persistent actions in the store,
			//which is clearly the reverse of what should happen.
			foreach (IAction action in actionMap.Values)
			{
				if (action.Persistent)
				{
					if (AppendActionToXmlModel(_actionModelXmlDoc, xmlActionModel, action))
						changed = true;
				}
			}

			if (changed)
			{
				if (!modelExists)
					this.GetActionModelsNode().AppendChild(xmlActionModel);
			}
			
			XmlElement xmlActionModelClone = (XmlElement)xmlActionModel.CloneNode(true);
	
			foreach (IAction action in actionMap.Values)
			{
				if (!action.Persistent)
					AppendActionToXmlModel(_actionModelXmlDoc, xmlActionModelClone, action);
			}

			return xmlActionModelClone;
		}

		/// <summary>
		/// Validates the entries in the xmlActionModel against the input set of actions.  If an entry
		/// in the xml model does not have a 'group-hint' attribute, the default one from the corresponding
		/// action is automatically inserted.
		/// </summary>
		/// <param name="xmlActionModel">the "action-model" to validate</param>
		/// <param name="actionMap">the set of actions against which to validate the "action-model"</param>
		/// <returns>a boolean indicating whether anything was modified</returns>
		private static bool ValidateXmlActionModel(XmlElement xmlActionModel, IDictionary<string, IAction> actionMap)
		{
			bool changed = false;

			foreach (XmlElement xmlAction in xmlActionModel.GetElementsByTagName("action"))
			{
				XmlAttribute groupHintNode = xmlAction.GetAttributeNode("group-hint");
				string id = xmlAction.GetAttribute("id");

				if (groupHintNode == null)
				{
					//Only automatically add the group-hint to the xml if a corresponding action is currently in memory.
					//otherwise, we don't know what it should be.
					if (actionMap.ContainsKey(id))
					{
						xmlAction.SetAttribute("group-hint", actionMap[id].GroupHint.Hint);
						changed = true;
					}
				}
			}

			return changed;
		}

		/// <summary>
		/// Gets an ordered list of <see cref="XmlElement"/> children of <paramref name="xmlActionModel"/>.
		/// </summary>
		private static List<XmlElement> GetActionNodeList(XmlElement xmlActionModel)
		{
			List<XmlElement> actionNodes = new List<XmlElement>();
			for (int n = 0; n < xmlActionModel.ChildNodes.Count; n++)
			{
				XmlElement xmlElement = xmlActionModel.ChildNodes[n] as XmlElement;
				if (xmlElement != null)
					actionNodes.Add(xmlElement);
			}
			return actionNodes;
		}

        /// <summary>
        /// Builds an in-memory action model from the specified XML model and the specified set of actions.
        /// The actions will be ordered according to the XML model.
        /// </summary>
        /// <param name="site">the action model site</param>
        /// <param name="xmlActionModel">an XML "action-model" node</param>
        /// <param name="actions">the set of actions that the model should contain</param>
        /// <returns>an <see cref="ActionModelNode"/> representing the root of the action model</returns>
		private static ActionModelRoot Build(string site, XmlElement xmlActionModel, IDictionary<string, IAction> actions)
        {
			ActionModelRoot model = new ActionModelRoot(site);
        	List<XmlElement> actionNodes = GetActionNodeList(xmlActionModel);

			// process xml model, inserting actions in order
			for (int i = 0; i < actionNodes.Count; i++)
			{
				XmlElement xmlAction = actionNodes[i];
				if (xmlAction.Name == "action")
				{
					string actionID = xmlAction.GetAttribute("id");
					if (actions.ContainsKey(actionID))
					{
						IAction action = actions[actionID];

						// update the action path from the xml
						ProcessXmlAction(xmlAction, action);

						// insert the action into the model
						model.InsertAction(action);
					}
				}
				else if (xmlAction.Name == "separator")
				{
                    Path separatorPath = ProcessSeparator(actionNodes, i, actions);

					// insert separator into model
					if (separatorPath != null)
						model.InsertSeparator(separatorPath);
				}
			}

			return model;
		}

		/// <summary>
		/// Processes an <paramref name="xmlAction"/> element in the XML model, deserializing the persisted values into the provided <paramref name="action"/>.
		/// </summary>
		private static void ProcessXmlAction(XmlElement xmlAction, IAction action)
		{
			string path = xmlAction.GetAttribute("path");
			string grouphint = xmlAction.GetAttribute("group-hint");

			action.Path = new ActionPath(path, action.ResourceResolver);
			action.GroupHint = new GroupHint(grouphint);

			bool available = true;
			string availableValue = xmlAction.GetAttribute("available");
			if (!string.IsNullOrEmpty(availableValue))
			{
				if (!bool.TryParse(availableValue, out available))
					available = true;
			}
			action.Available = available;

			if (action is IClickAction)
			{
				IClickAction clickAction = (IClickAction) action;

				XKeys keyStroke = XKeys.None;
				string keystrokeValue = xmlAction.GetAttribute("keystroke");
				if (!string.IsNullOrEmpty(keystrokeValue))
				{
					if (!XKeysConverter.TryParseInvariant(keystrokeValue, out keyStroke))
						Platform.Log(LogLevel.Debug, "Invalid value for attribute keystroke for action {0}", action.ActionID);
				}
				clickAction.KeyStroke = keyStroke;
			}
		}

		/// <summary>
        /// Processes a separator node in the XML action model.
        /// </summary>
        /// <param name="actionNodes"></param>
        /// <param name="i"></param>
        /// <param name="actions"></param>
        private static Path ProcessSeparator(IList<XmlElement> actionNodes, int i, IDictionary<string, IAction> actions)
        {
            // a separator at the beginning or end of the list is not valid - ignore it
            if (i == 0 || i == actionNodes.Count - 1)
                return null;

            // get the actions appearing immediately before and after the separator
			XmlElement preXmlAction = GetFirstAdjacentAction(actionNodes, i, -1);
			XmlElement postXmlAction = GetFirstAdjacentAction(actionNodes, i, 1);

			// if either could not be found, the separator can be ignored,
			// because it would be located at the edge of the menu/toolbar
			if (preXmlAction == null || postXmlAction == null)
				return null;

            // use these to determine the location of the separator, based on the longest common path
            string commonPath = GetLongestCommonPath(preXmlAction.GetAttribute("path"), postXmlAction.GetAttribute("path"));

            // in order to construct the separator's Path object, we need a valid resource resolver
            // search both backward and forward through the model to find the first adjacent actions
            // that exist in the dictionary, in order to "borrow" their resource resolvers

            // get the first action before and after separator for which an IAction exists
            preXmlAction = GetFirstExistingAdjacentAction(actionNodes, i, -1, actions);
            postXmlAction = GetFirstExistingAdjacentAction(actionNodes, i, 1, actions);

            // if either could not be found, the separator can be ignored,
            // because it would also be located at the edge of the menu/toolbar
            if (preXmlAction == null || postXmlAction == null)
                return null;

            // get the corresponding IActions, which are guaranteed to exist,
            // so that we can use their Resource resolvers to localize paths
            IAction preAction = actions[preXmlAction.GetAttribute("id")];
            IAction postAction = actions[postXmlAction.GetAttribute("id")];

            // if either of the adjacent existing actions do not start with the common path,
            // then the separator can be ignored because it would be located at an edge
            if (!(new Path(preXmlAction.GetAttribute("path"), preAction.ResourceResolver)).
                StartsWith(new Path(commonPath, preAction.ResourceResolver)))
                return null;
            if (!(new Path(postXmlAction.GetAttribute("path"), postAction.ResourceResolver)).
                StartsWith(new Path(commonPath, postAction.ResourceResolver)))
                return null;

            // given that we now have both the common path and an appropriate resource resolver,
            // we can construct the separator path (using either the pre or post resource resolver),
            // appending a segment to represent the separator itself
            Path separatorPath = (new Path(commonPath, preAction.ResourceResolver)).Append(new Path(string.Format("_s{0}", i)));

			return separatorPath;
        }

        /// <summary>
        /// Gets the longest common path between two paths.
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private static string GetLongestCommonPath(string path1, string path2)
        {
            return (new Path(StringUtilities.EmptyIfNull(path1))).GetCommonPath(new Path(StringUtilities.EmptyIfNull(path2))).ToString();
        }

        /// <summary>
        /// Finds the first XML action element adjacent to the start position for which an action exists. 
        /// </summary>
        /// <param name="actionNodes"></param>
        /// <param name="start"></param>
        /// <param name="increment"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        private static XmlElement GetFirstExistingAdjacentAction(IList<XmlElement> actionNodes, int start, int increment,
            IDictionary<string, IAction> actions)
        {
            for (int i = start + increment; i >= 0 && i < actionNodes.Count; i += increment)
            {
                XmlElement actionNode = actionNodes[i];

                IAction action;
                if (actions.TryGetValue(actionNode.GetAttribute("id"), out action))
                    return actionNode;
            }
            return null;
        }

		/// <summary>
		/// Finds the first XML action element adjacent to the start position (i.e. separators excluded)
		/// </summary>
		private static XmlElement GetFirstAdjacentAction(IList<XmlElement> actionNodes, int start, int increment)
		{
			for (int i = start + increment; i >= 0 && i < actionNodes.Count; i += increment)
			{
				XmlElement actionNode = actionNodes[i];

				if (actionNode.Name == "action")
					return actionNode;
			}
			return null;
		}

        /// <summary>
        /// Appends the specified action to the specified XML action model.  The "group-hint"
		/// attribute of the action to be inserted is compared with the "group-hint" of the
		/// actions in the xml model and an appropriate place to insert the action is determined
		/// based on the MatchScore method of the <see cref="GroupHint"/>.
        /// </summary>
        /// <param name="xmlActionModel">the "action-model" node to insert an action into</param>
        /// <param name="action">the action to be inserted</param>
		/// <returns>a boolean indicating whether anything was added/removed/modified</returns>
        private static bool AppendActionToXmlModel(XmlDocument document, XmlElement xmlActionModel, IAction action)
        {
			if (null != FindXmlAction(action.ActionID, xmlActionModel))
				return false;
			
			XmlNode insertionPoint = null;
			int currentGroupScore = 0;

			foreach (XmlElement xmlAction in xmlActionModel.GetElementsByTagName("action"))
			{
				string hint = xmlAction.GetAttribute("group-hint");
				GroupHint groupHint = new GroupHint(hint);

				int groupScore = action.GroupHint.MatchScore(groupHint);
				if (Math.Abs(groupScore) >= Math.Abs(currentGroupScore))
				{
					insertionPoint = xmlAction;
					currentGroupScore = groupScore;
				}
			}
						
			XmlElement newXmlAction = CreateXmlAction(document, action);
			
			if (insertionPoint != null)
				xmlActionModel.InsertAfter(newXmlAction, insertionPoint);
			else
				xmlActionModel.AppendChild(newXmlAction);

			return true;
		}

		private XmlElement GetActionModelsNode()
		{
			return (XmlElement)_actionModelXmlDoc.GetElementsByTagName("action-models")[0];
		}

		#endregion

	}
}

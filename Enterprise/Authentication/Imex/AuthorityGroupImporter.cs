﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using ClearCanvas.Enterprise.Core;
using System.IO;
using ClearCanvas.Enterprise.Authentication.Brokers;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Common;
using ClearCanvas.Common.Authorization;

namespace ClearCanvas.Enterprise.Authentication.Imex
{

    /// <summary>
    /// Imports authority groups from plugins that define extensions to <see cref="DefineAuthorityGroupsExtensionPoint"/>.
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IApplicationRoot"/> so that it may be run stand-alone from a console.  However,
    /// it may also be used as a utility class to be invoked by other means.
    /// </remarks>
    [ExtensionOf(typeof(ApplicationRootExtensionPoint))]
    public class AuthorityGroupImporter : IApplicationRoot
    {
        /// <summary>
        /// Import authority groups from extensions of <see cref="DefineAuthorityGroupsExtensionPoint"/>.
        /// </summary>
        /// <remarks>
        /// Creates any authority groups that do not already exist.
        /// This method performs an additive import.  It will never remove an existing authority group or
        /// remove authority tokens from an existing group.
        /// </remarks>
        /// <param name="context"></param>
        public IList<AuthorityGroup> ImportFromPlugins(IUpdateContext context)
        {
            AuthorityGroupDefinition[] groupDefs = AuthorityGroupSetup.GetDefaultAuthorityGroups();
            return Import(groupDefs, context);
        }

		/// <summary>
		/// Import authority groups.
		/// </summary>
		/// <remarks>
		/// Creates any authority groups that do not already exist.
		/// This method performs an additive import.  It will never remove an existing authority group or
		/// remove authority tokens from an existing group.
		/// </remarks>
		/// <param name="groupDefs"></param>
		/// <param name="context"></param>
		public IList<AuthorityGroup> Import(IEnumerable<AuthorityGroupDefinition> groupDefs, IUpdateContext context)
        {
            // first load all the existing tokens into memory
            // there should not be that many tokens ( < 500), so this should not be a problem
            IAuthorityTokenBroker tokenBroker = context.GetBroker<IAuthorityTokenBroker>();
            IList<AuthorityToken> existingTokens = tokenBroker.FindAll();

            // load existing groups
            IAuthorityGroupBroker groupBroker = context.GetBroker<IAuthorityGroupBroker>();
            IList<AuthorityGroup> existingGroups = groupBroker.FindAll();

            foreach (AuthorityGroupDefinition groupDef in groupDefs)
            {
                AuthorityGroup group = CollectionUtils.SelectFirst(existingGroups,
                    delegate(AuthorityGroup g) { return g.Name == groupDef.Name; });

                // if group does not exist, create it
                if (group == null)
                {
                    group = new AuthorityGroup();
                    group.Name = groupDef.Name;
                    context.Lock(group, DirtyState.New);
                    existingGroups.Add(group);
                }

                // process all token nodes contained in group
                foreach (string tokenName in groupDef.Tokens)
                {
                    AuthorityToken token = CollectionUtils.SelectFirst(existingTokens,
                        delegate(AuthorityToken t) { return t.Name == tokenName; });

                    // ignore non-existent tokens
                    if (token == null)
                        continue;

                    // add the token to the group
                    group.AuthorityTokens.Add(token);
                }
            }

            return existingGroups;
        }

        #region IApplicationRoot Members

        public void RunApplication(string[] args)
        {
            using (PersistenceScope scope = new PersistenceScope(PersistenceContextType.Update))
            {
                ((IUpdateContext) PersistenceScope.CurrentContext).ChangeSetRecorder.OperationName = this.GetType().FullName;
                ImportFromPlugins((IUpdateContext)PersistenceScope.CurrentContext);

                scope.Complete();
            }
        }

        #endregion

    }
}

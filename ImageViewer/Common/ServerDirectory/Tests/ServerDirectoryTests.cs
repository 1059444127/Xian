﻿#region License

// Copyright (c) 2012, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

#if UNIT_TESTS

using System;
using System.Linq;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom.ServiceModel.Query;
using ClearCanvas.ImageViewer.Common.Configuration.Tests;
using ClearCanvas.ImageViewer.Common.DicomServer;
using ClearCanvas.ImageViewer.Common.StudyManagement;
using NUnit.Framework;

namespace ClearCanvas.ImageViewer.Common.ServerDirectory.Tests
{
    [TestFixture]
    public class ServerDirectoryTests
    {
        public void Initialize1()
        {
            DicomServer.Tests.DicomServerTestServiceProvider.Reset();
            StudyManagement.Tests.StudyStoreTestServiceProvider.Reset();

            Platform.SetExtensionFactory(new UnitTestExtensionFactory
                                             {
                                                 {
                                                     typeof (ServiceProviderExtensionPoint),
                                                     typeof (TestSystemConfigurationServiceProvider)
                                                     },
                                                 {
                                                     typeof (ServiceProviderExtensionPoint),
                                                     typeof (ServerDirectoryTestServiceProvider)
                                                     },
                                                 {
                                                     typeof (ServiceProviderExtensionPoint),
                                                     typeof (DicomServer.Tests.DicomServerTestServiceProvider)
                                                     },
                                                 {
                                                     typeof (ServiceProviderExtensionPoint),
                                                     typeof (StudyManagement.Tests.StudyStoreTestServiceProvider)
                                                     }
                                             });

            //Force IsSupported to be re-evaluated.
            StudyStore.InitializeIsSupported();
        }

        public void Initialize2()
        {
            DicomServer.Tests.DicomServerTestServiceProvider.Reset();

            Platform.SetExtensionFactory(new UnitTestExtensionFactory
                                             {
                                                 {
                                                     typeof (ServiceProviderExtensionPoint),
                                                     typeof (TestSystemConfigurationServiceProvider)
                                                     },
                                                 {
                                                     typeof (ServiceProviderExtensionPoint),
                                                     typeof (ServerDirectoryTestServiceProvider)
                                                     },
                                                 {
                                                     typeof (ServiceProviderExtensionPoint),
                                                     typeof (DicomServer.Tests.DicomServerTestServiceProvider)
                                                     }
                                             });

            //Force IsSupported to be re-evaluated.
            StudyStore.InitializeIsSupported();
        }

        [Test]
        public void TestGetLocalServer()
        {
            Initialize1();

            var local = ServerDirectory.GetLocalServer();
            Assert.AreEqual("<local>", local.Name);
            Assert.AreEqual("CLEARCANVAS", local.AETitle); //default value from DicomServerSettings.
            Assert.AreEqual("localhost", local.ScpParameters.HostName);
            Assert.AreEqual(104, local.ScpParameters.Port);
            Assert.IsTrue(local.IsLocal);

            var contract = local.ToDataContract();
            Assert.AreEqual("<local>", contract.Server.Name);
            Assert.AreEqual("CLEARCANVAS", contract.Server.AETitle);
            Assert.AreEqual("localhost", contract.Server.ScpParameters.HostName);
            Assert.AreEqual(104, contract.Server.ScpParameters.Port);

            DicomServer.DicomServer.UpdateConfiguration(new DicomServerConfiguration
                                                            {AETitle = "Local2", HostName = "::1", Port = 104});

            local = ServerDirectory.GetLocalServer();
            Assert.AreEqual("<local>", local.Name);
            Assert.AreEqual("Local2", local.AETitle);
            Assert.AreEqual("::1", local.ScpParameters.HostName);
            Assert.AreEqual(104, local.ScpParameters.Port);
            Assert.IsTrue(local.IsLocal);

            contract = local.ToDataContract();
            Assert.AreEqual("<local>", contract.Server.Name);
            Assert.AreEqual("Local2", contract.Server.AETitle);
            Assert.AreEqual("::1", contract.Server.ScpParameters.HostName);
            Assert.AreEqual(104, contract.Server.ScpParameters.Port);
        }

        [Test]
        public void TestGetPriorsServers()
        {
            Initialize1();

            var priorsServers = ServerDirectory.GetPriorsServers(true);
            Assert.AreEqual(2, priorsServers.Count);
            Assert.AreEqual(1, priorsServers.Count(s => s.IsLocal));

            priorsServers = ServerDirectory.GetPriorsServers(false);
            Assert.AreEqual(1, priorsServers.Count);
            Assert.AreEqual(0, priorsServers.Count(s => s.IsLocal));
        }

        [Test]
        public void TestGetServersByName()
        {
            Initialize1();

            var name1 = ServerDirectory.GetRemoteServerByName("Name1");
            Assert.IsNotNull(name1);
            var name2 = ServerDirectory.GetRemoteServerByName("Name2");
            Assert.IsNotNull(name2);
            Assert.IsNull(ServerDirectory.GetRemoteServerByName(null));
            Assert.IsNull(ServerDirectory.GetRemoteServerByName(String.Empty));
            Assert.IsNull(ServerDirectory.GetRemoteServerByName("abc"));
        }

        [Test]
        public void TestGetServersByAE()
        {
            Initialize1();

            var all = ServerDirectory.GetRemoteServers();
            Assert.AreEqual(2, all.Count);

            var ae1 = ServerDirectory.GetRemoteServersByAETitle("AE1");
            Assert.AreEqual(1, ae1.Count);
            var ae2 = ServerDirectory.GetRemoteServersByAETitle("AE2");
            Assert.AreEqual(1, ae2.Count);
            Assert.AreEqual(0, ServerDirectory.GetRemoteServersByAETitle(null).Count);
            Assert.AreEqual(0, ServerDirectory.GetRemoteServersByAETitle(String.Empty).Count);
            Assert.AreEqual(0, ServerDirectory.GetRemoteServersByAETitle("abc").Count);
        }

        [Test]
        public void TestServiceNodeLocalServices()
        {
            Initialize1();

            var local = ServerDirectory.GetLocalServer();
            Assert.IsTrue(local.IsSupported<IStudyRootQuery>());
            Assert.IsNotNull(local.GetService<IStudyRootQuery>());

            Assert.IsTrue(local.IsSupported<IStudyStoreQuery>());
            Assert.IsNotNull(local.GetService<IStudyStoreQuery>());

            Initialize2();

            Assert.IsFalse(local.IsSupported<IStudyRootQuery>());
            try
            {
                var service = local.GetService<IStudyRootQuery>();
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }

            Assert.IsFalse(local.IsSupported<IStudyStoreQuery>());
            try
            {
                var service = local.GetService<IStudyStoreQuery>();
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }

        [Test]
        public void TestServiceNodeRemoteServices()
        {
            Initialize1();

            var remote = ServerDirectory.GetRemoteServerByName("Name1");
            Assert.IsFalse(remote.IsSupported<IStudyStoreQuery>());
            Assert.IsTrue(remote.IsSupported<IStudyRootQuery>());
            Assert.IsNotNull(remote.GetService<IStudyRootQuery>());

            remote = ServerDirectory.GetRemoteServerByName("Name2");
            Assert.IsFalse(remote.IsSupported<IStudyStoreQuery>());
            Assert.IsFalse(remote.IsSupported<IStudyRootQuery>());

            try
            {
                var service = remote.GetService<IStudyRootQuery>();
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }
    }
}

#endif
﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Storage.Test.Queue
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model.ResourceModel;
    using Microsoft.WindowsAzure.Management.Storage.Queue;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class GetAzureStorageQueueTest : StorageQueueTestBase
    {
        public GetAzureStorageQueueCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new GetAzureStorageQueueCommand(queueMock)
                {
                    CommandRuntime = new MockCommandRuntime()
                };
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void ListQueuesByNameWithEmptyNameTest()
        {
            List<CloudQueue> queueList = command.ListQueuesByName("").ToList();
            Assert.AreEqual(0, queueList.Count);

            AddTestQueues();
            queueList = command.ListQueuesByName("").ToList();
            Assert.AreEqual(2, queueList.Count);
        }

        [TestMethod]
        public void ListQueuesByNameWithWildCardTest()
        {
            List<CloudQueue> queueList = command.ListQueuesByName("te*t").ToList();
            Assert.AreEqual(0, queueList.Count);

            AddTestQueues();

            queueList = command.ListQueuesByName("te*t").ToList();
            Assert.AreEqual(2, queueList.Count);

            queueList = command.ListQueuesByName("tx*t").ToList();
            Assert.AreEqual(0, queueList.Count);

            queueList = command.ListQueuesByName("t?st").ToList();
            Assert.AreEqual(1, queueList.Count);
            Assert.AreEqual("test", queueList[0].Name);
        }

        [TestMethod]
        public void ListQueuesByNameWithInvalidNameTest()
        {
            string invalidName = "a";
            AssertThrows<ArgumentException>(() => command.ListQueuesByName(invalidName).ToList(),
                String.Format(Resources.InvalidQueueName, invalidName));
            invalidName = "xx%%d";
            AssertThrows<ArgumentException>(() => command.ListQueuesByName(invalidName).ToList(),
                String.Format(Resources.InvalidQueueName, invalidName));
        }

        [TestMethod]
        public void ListQueuesByNameWitNotExistsQueueTest()
        {
            string notExistingName = "abcdefg";
            AssertThrows<ResourceNotFoundException>(() => command.ListQueuesByName(notExistingName).ToList(),
                String.Format(Resources.QueueNotFound, notExistingName));
        }

        [TestMethod]
        public void ListQueuesByNameSuccessfullyTest()
        {
            AddTestQueues();

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            List<CloudQueue> queueList = command.ListQueuesByName("text").ToList();
            Assert.AreEqual(1, queueList.Count);
            Assert.AreEqual("text", queueList[0].Name);
        }

        [TestMethod]
        public void ListQueuesByPrefixWithEmptyPrefixTest()
        {
            AssertThrows<ArgumentException>(() => command.ListQueuesByPrefix(String.Empty),
                String.Format(Resources.InvalidQueueName, String.Empty));
        }

        [TestMethod]
        public void ListQueuesByPrefixWithInvalidPrefixTest()
        {
            string prefix = "?";
            AssertThrows<ArgumentException>(() => command.ListQueuesByPrefix(prefix).ToList(), String.Format(Resources.InvalidQueueName, prefix));
        }

        [TestMethod]
        public void ListQueuesByPrefixSuccessfullyTest()
        {
            AddTestQueues();

            List<CloudQueue> queueList = command.ListQueuesByPrefix("te").ToList();
            Assert.AreEqual(2, queueList.Count);

            command.ListQueuesByPrefix("tes");
            Assert.AreEqual(2, queueList.Count);
            Assert.IsTrue(queueList.Any(item => item.Name == "test"));
            Assert.IsTrue(queueList.Any(item => item.Name == "text"));

            queueList = command.ListQueuesByPrefix("testx").ToList();
            Assert.AreEqual(0, queueList.Count);
        }

        [TestMethod]
        public void WriteQueueWithCountTest()
        {
            command.WriteQueueWithCount(null);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.WriteQueueWithCount(queueMock.queueList);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            AddTestQueues();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.WriteQueueWithCount(queueMock.queueList);
            Assert.AreEqual(2, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
        }

        [TestMethod]
        public void ExecuteCommandGetQueueTest()
        {
            AddTestQueues();
            command.Name = "test";
            command.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            AzureStorageQueue queue = (AzureStorageQueue)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(command.Name, queue.Name);
        }
    }
}

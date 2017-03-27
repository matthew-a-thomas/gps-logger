﻿using System.Threading.Tasks;
using Common.Extensions;
using Common.LocalStorage;
using GPSLogger.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Controllers
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class HMACKeyControllerClass
    {
        private static IStorage CreateStore() => new MemoryStorage();

        [TestClass]
        public class GetAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameter()
            {
                var mockedStore = new Mock<IStorage>();
                var controller = new HMACKeyController(mockedStore.Object);
                await controller.GetAsync();
            }

            [TestMethod]
            public async Task ReturnsFalseAtFirst()
            {
                var store = CreateStore();
                var controller = new HMACKeyController(store);
                var get = await controller.GetAsync();
                Assert.IsFalse(get);
            }

            [TestMethod]
            public async Task ReturnsTrueWhenSet()
            {
                var store = CreateStore();
                var controller = new HMACKeyController(store);
                await controller.PostAsync(new HMACKeyController.PostParameters
                {
                    NewKey = (new byte[100]).ToHexString()
                });
                var get = await controller.GetAsync();
                Assert.IsTrue(get);
            }
        }
    }
}

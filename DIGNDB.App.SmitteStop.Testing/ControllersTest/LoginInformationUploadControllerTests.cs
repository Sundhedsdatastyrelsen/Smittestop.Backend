using DIGNDB.App.SmitteStop.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Dto;

namespace DIGNDB.App.SmitteStop.Testing.ControllersTest
{
    [TestFixture]
    public class LoginInformationControllerTests
    {
        private MockRepository _mockRepository;

        private Mock<ILogger<LoginInformationUploadController>> _mockLogger;
        private Mock<ILoginInformationRepository> _mockLoginInformationRepository;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Loose);
            
            _mockLogger = _mockRepository.Create<ILogger<LoginInformationUploadController>>();
            _mockLoginInformationRepository = _mockRepository.Create<ILoginInformationRepository>();
        }

        private LoginInformationUploadController CreateLoginInformationUploadController()
        {
            return new LoginInformationUploadController(_mockLogger.Object, _mockLoginInformationRepository.Object);
        }

        //public Mock<HttpContext> MakeFakeContext(int numberOfFiles, bool emptyFiles)
        //{
        //    var mockRequest = new Mock<HttpRequest>();
        //    var mockedFileCollection = new FormFileCollection();
        //    string fileContent = "This is a dummy file";
        //    if (emptyFiles)
        //    {
        //        fileContent = "";
        //    }
        //    for (int i = 0; i < numberOfFiles; i++)
        //    {
        //        mockedFileCollection.Add(new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)), 0, fileContent.Length, "Data", "dummy.txt"));
        //    }
        //    mockRequest.Setup(c => c.Form).Returns(new FormCollection(new Dictionary<string, StringValues>(), mockedFileCollection));
        //    var mockContext = new Mock<HttpContext>();
        //    mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

        //    return mockContext;
        //}

        [Ignore("Turn off a logs")]
        [Test]
        public async Task UploadLoginInformation_OneEntry_ShouldReturn200()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(Json));
            var httpContext = new DefaultHttpContext
            {
                Request = { Body = stream, ContentLength = stream.Length }
            };
            var loginInformationUploadController = CreateLoginInformationUploadController();
            loginInformationUploadController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await loginInformationUploadController.UploadLoginInformation();

            // Assert
            Assert.AreEqual(200, ((ObjectResult)result).StatusCode);
        }

        [Ignore("Turn off a logs")]
        [Test]
        public async Task UploadLoginInformation_MoreThanOneEntry_ShouldReturn500()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(Jsons));
            var httpContext = new DefaultHttpContext
            {
                Request = { Body = stream, ContentLength = stream.Length }
            };
            var loginInformationUploadController = CreateLoginInformationUploadController();
            loginInformationUploadController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await loginInformationUploadController.UploadLoginInformation();

            // Assert
            Assert.AreEqual(500, ((StatusCodeResult)result).StatusCode);
        }

        [Ignore("Turn off a logs")]
        [Test]
        public async Task UploadLoginInformation_InValidJson_ShouldReturn200()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(InvalidJson));
            var httpContext = new DefaultHttpContext
            {
                Request = { Body = stream, ContentLength = stream.Length }
            };
            var loginInformationUploadController = CreateLoginInformationUploadController();
            loginInformationUploadController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await loginInformationUploadController.UploadLoginInformation();

            // Assert
            Assert.AreEqual(400, ((BadRequestObjectResult)result).StatusCode);
        }

        [Ignore("Turn off a logs")]
        [Test]
        public async Task UploadLoginInformation_EmptyJson_ShouldReturn200()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(EmptyJson));
            var httpContext = new DefaultHttpContext
            {
                Request = { Body = stream, ContentLength = stream.Length }
            };
            var loginInformationUploadController = CreateLoginInformationUploadController();
            loginInformationUploadController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await loginInformationUploadController.UploadLoginInformation();

            // Assert
            Assert.AreEqual(500, ((StatusCodeResult)result).StatusCode);
        }

        private const string Json = @"
[
    {
        ""tags"": {
        ""source"": ""smittestop_nc_dashboard""
    },
    ""events"": [
            {
                ""attributes"": {
                    ""negativ"": ""138"",
                    ""blokeret"": ""0"",
                    ""positiv"": ""62"",
                    ""fejl"": ""0"",
                    ""timestamp"": ""1618272000000""
                },
                ""timestamp"": ""2021-04-13T00:52:00.738Z""
            }
        ]
    }
]
";

        private const string Jsons = @"
[
    {
        ""tags"": {
        ""source"": ""smittestop_nc_dashboard""
    },
    ""events"": [
            {
                ""attributes"": {
                    ""negativ"": ""138"",
                    ""blokeret"": ""0"",
                    ""positiv"": ""62"",
                    ""fejl"": ""0"",
                    ""timestamp"": ""1618272000000""
                },
                ""timestamp"": ""2021-04-13T00:52:00.738Z""
            },
                        {
                ""attributes"": {
                    ""negativ"": ""138"",
                    ""blokeret"": ""0"",
                    ""positiv"": ""62"",
                    ""fejl"": ""0"",
                    ""timestamp"": ""1618272000000""
                },
                ""timestamp"": ""2021-04-13T00:52:00.738Z""
            }
        ]
    }
]
";
        private const string InvalidJson = @"
[
    {
        ""tags"": {
        ""source"": ""smittestop_nc_dashboard""
    },
    ""events"": 
            {
                ""attributes"": {
                    ""negativ"": ""138"",
                    ""blokeret"": ""0"",
                    ""positiv"": ""62"",
                    ""fejl"": ""0"",
                    ""timestamp"": ""1618272000000""
                },
                ""timestamp"": ""2021-04-13T00:52:00.738Z""
            }
        ]
    }
]
";

        private const string EmptyJson = @"
[    
]
";
    }
}

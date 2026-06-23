//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using anamoly_detection_api.Controllers;
//using anamoly_detection_api.Services;
//using anamoly_detection_api.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;

//namespace UnitTest
//{
//    [TestClass]
//    public class AuthControllerTest
//    {
//        private Mock<ILoginService> _mockService;
//        private AuthController _controller;

//        [TestInitialize]
//        public void Setup()
//        {
//            // Create mock service
//            _mockService = new Mock<ILoginService>();

//            // Inject mock into controller
//            _controller = new AuthController(_mockService.Object);
//        }

//        [TestMethod]
//        public void GetLogin_ShouldReturnTrue()
//        {
//            // Act
//            var result = _controller.GetLogin();

//            // Assert
//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public async Task Login_ShouldReturnOk_WhenLoginSuccessful()
//        {
//            // Arrange
//            var dto = new LoginDto
//            {
//                Email = "admin@test.com",
//                Password = "1234"
//            };

//            var loginResponse = new LoginResultDto
//            {
//                IsSuccess = true,
//                Message = "Login successful",
//                User = new LoginDto
//                {
                    
//                    Email = "admin@test.com",
//                }
//            };

//            // Mock service behavior
//            _mockService
//                .Setup(x => x.LoginAsync(dto))
//                .ReturnsAsync(loginResponse);

//            // Act
//            var result = await _controller.Login(dto);

//            // Assert
//            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

//            var okResult = result as OkObjectResult;

//            Assert.IsNotNull(okResult);
//        }

//        [TestMethod]
//        public async Task Login_ShouldReturnUnauthorized_WhenLoginFails()
//        {
//            // Arrange
//            var dto = new LoginDto
//            {
//                Email = "wrong@test.com",
//                Password = "wrong"
//            };

//            var loginResponse = new LoginResultDto
//            {
//                IsSuccess = false,
//                Message = "Invalid credentials"
//            };

//            // Mock failed login
//            _mockService
//                .Setup(x => x.LoginAsync(dto))
//                .ReturnsAsync(loginResponse);

//            // Act
//            var result = await _controller.Login(dto);

//            // Assert
//            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));

//            var unauthorizedResult =
//                result as UnauthorizedObjectResult;

//            Assert.IsNotNull(unauthorizedResult);
//        }
//    }
//}
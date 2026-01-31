using System;
using Xunit;
using PutZige.API.Extensions;

namespace PutZige.API.Tests.Extensions
{
    public class ModelStateExtensionsTests
    {
        /// <summary>
        /// Converts various field names to expected camelCase.
        /// </summary>
        [Theory]
        [InlineData("Email", "email")]
        [InlineData("Password", "password")]
        [InlineData("DisplayName", "displayName")]
        [InlineData("user.email", "email")]
        [InlineData("user.password", "password")]
        [InlineData("x", "x")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void ToCamelCaseField_ConvertsCorrectly(string? input, string expected)
        {
            var result = ModelStateExtensions.ToCamelCaseField(input);
            Assert.Equal(expected, result);
        }
    }
}

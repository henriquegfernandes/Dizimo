using System;
using System.IO;
using System.Threading.Tasks;
using Dizimo.Core.Services;
using Xunit;

namespace Dizimo.Tests
{
    public class AuthServiceTests : IDisposable
    {
        private readonly string _path;
        private readonly AuthService _auth;

        public AuthServiceTests()
        {
            _path = Path.Combine(Path.GetTempPath(), $"users_{Guid.NewGuid():N}.json");
            _auth = new AuthService(_path);
        }

        [Fact]
        public void Create_And_Authenticate_User()
        {
            var ok = _auth.CreateUser("u1", "p@ss");
            Assert.True(ok);
            var auth = _auth.Authenticate("u1", "p@ss");
            Assert.True(auth);
            Assert.NotNull(_auth.CurrentUser);
            Assert.Equal("u1", _auth.CurrentUser.Username, ignoreCase: true);
        }

        public void Dispose()
        {
            try { File.Delete(_path); } catch { }
        }
    }
}

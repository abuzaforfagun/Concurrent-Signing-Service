using System;
using System.Text;
using Xunit;

namespace ConcurrentSigning.Cryptography.Tests
{
    public class SymmetricEncryptionTests
    {
        [Fact]
        public void EncryptedDataShouldBeDecrypted()
        {
            var data = "hello";
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var encryptedData = SymmetricEncryption.Encrypt(data, key);
            var decryptedData = SymmetricEncryption.Decrypt(encryptedData, key);

            Assert.Equal(data, decryptedData);

        }
    }
}
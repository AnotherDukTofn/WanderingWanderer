using Game.Logic.Shared;
using NUnit.Framework;
using Game.Logic.Shared.Structs;

namespace Tests.EditMode.Spells
{
    [TestFixture]
    public class PotencyRefTests
    {
        [Test]
        public void GetValue_Coefficient08_Potency50_Returns40()
        {
            // Arrange
            // Lưu ý: Đảm bảo bạn đã sửa typo 'coefficent' thành 'coefficient' trong Structs.cs
            var potencyRef = new PotencyRef
            {
                element = Element.Fire,
                coefficient = 0.8f 
            };

            // Act: Gọi method GetValue với potency là 50
            float result = potencyRef.GetValue(50f);

            // Assert: Verify kết quả là 40 (với sai số nhỏ cho phép của float)
            Assert.AreEqual(40f, result, 0.001f, "GetValue phải trả về kết quả là coefficient nhân với potency");
        }
        
        [TestCase(1.5f, 100f, 150f)]
        [TestCase(0f, 50f, 0f)]
        [TestCase(1f, 75f, 75f)]
        public void GetValue_VariousCases_ReturnsCorrectResult(float specCoefficient, float potency, float expected)
        {
            // Test thêm nhiều trường hợp khác để đảm bảo logic luôn đúng
            var potencyRef = new PotencyRef
            {
                element = Element.Water,
                coefficient = specCoefficient
            };

            Assert.AreEqual(expected, potencyRef.GetValue(potency), 0.001f);
        }
    }
}
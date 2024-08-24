using FluentAssertions;

namespace Application.UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var a = 1; var b =2;
            var c =3;

            c.Equals(a+b);
        }
    }
}
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EditorColorPreview.Test
{
    [TestClass]
    public class CSharpColors
    {
        [DataTestMethod]
        [DataRow("new Color(255, 0, 0, 255)")]
        [DataRow("new Color(0, 128, 255)")]
        [DataRow("new Color(0,0,0,0)")]
        [DataRow("new Color( 255 , 255 , 255 )")]
        [DataRow("new(255, 0, 0, 255)")]
        [DataRow("new(0, 128, 255)")]
        [DataRow("new Color(1.0f, 0.5f, 0.0f)")]
        [DataRow("new Color(1.0f, 0.5f, 0.0f, 1.0f)")]
        [DataRow("new(0.5f, 0.5f, 0.5f, 1.0f)")]
        [DataRow("new Color(0.0f, 0.0f, 0.0f, 0.0f)")]
        [DataRow("var c = new Color(255, 0, 0);")]
        [DataRow("private static readonly Color Foreground = new(0, 0, 0, 255);")]
        public void CSharp_New_Should_Match(string input)
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor(input);
            Assert.AreEqual(1, matches.Count());
        }

        [DataTestMethod]
        [DataRow("public static readonly Color PageBackground = Rgba(11, 15, 26);")]
        [DataRow("Color c = Rgba(255, 0, 0, 128);")]
        [DataRow("Color c = GetColor(128, 64, 32);")]
        [DataRow("Color c = MyHelper(1.0f, 0.5f, 0.0f);")]
        public void CSharp_ColorTyped_Call_Should_Match(string input)
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor(input);
            Assert.IsTrue(matches.Count() >= 1, $"Expected at least 1 match for: {input}");
        }

        [DataTestMethod]
        [DataRow("int x = Rgba(11, 15, 26);")]
        [DataRow("var x = SomeMethod(1, 2, 3);")]
        [DataRow("string s = Format(10, 20, 30);")]
        public void CSharp_NonColor_Call_Should_Not_Match(string input)
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor(input);
            Assert.AreEqual(0, matches.Count());
        }

        [DataTestMethod]
        [DataRow("// new Color(255, 0, 0)")]
        [DataRow("// new(0, 0, 0, 0)")]
        [DataRow("// new Color(1.0f, 0.5f, 0.0f)")]
        [DataRow("// Matches: new Color(255,0,0,255), new Color(1.0f, 0.5f, 0.0f)")]
        [DataRow("// Color c = Rgba(11, 15, 26);")]
        public void CSharp_Comments_Should_Match(string input)
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor(input);
            Assert.IsTrue(matches.Count() >= 1);
        }

        [DataTestMethod]
        [DataRow("new Color(255, 0, 0, 255)", 255, 255, 0, 0)]
        [DataRow("new Color(0, 128, 255)", 255, 0, 128, 255)]
        [DataRow("new Color(0, 0, 0, 0)", 0, 0, 0, 0)]
        [DataRow("new Color(255, 255, 255, 255)", 255, 255, 255, 255)]
        [DataRow("new Color(128, 64, 32)", 255, 128, 64, 32)]
        [DataRow("new(255, 0, 0, 255)", 255, 255, 0, 0)]
        [DataRow("new(0, 128, 255)", 255, 0, 128, 255)]
        [DataRow("new(0, 0, 0, 0)", 0, 0, 0, 0)]
        [DataRow("Rgba(11, 15, 26)", 255, 11, 15, 26)]
        [DataRow("Rgba(255, 0, 0, 128)", 128, 255, 0, 0)]
        public void CSharp_Int_Color_Values(string input, int expectedA, int expectedR, int expectedG, int expectedB)
        {
            Color actual = ColorUtils.HtmlToColor(input);
            Assert.AreEqual(expectedA, actual.A, $"Alpha mismatch for {input}");
            Assert.AreEqual(expectedR, actual.R, $"Red mismatch for {input}");
            Assert.AreEqual(expectedG, actual.G, $"Green mismatch for {input}");
            Assert.AreEqual(expectedB, actual.B, $"Blue mismatch for {input}");
        }

        [DataTestMethod]
        [DataRow("new Color(1.0f, 0.0f, 0.0f)", 255, 255, 0, 0)]
        [DataRow("new Color(0.0f, 1.0f, 0.0f)", 255, 0, 255, 0)]
        [DataRow("new Color(0.0f, 0.0f, 1.0f)", 255, 0, 0, 255)]
        [DataRow("new Color(1.0f, 1.0f, 1.0f, 1.0f)", 255, 255, 255, 255)]
        [DataRow("new Color(0.0f, 0.0f, 0.0f, 0.0f)", 0, 0, 0, 0)]
        [DataRow("new Color(0.5f, 0.5f, 0.5f)", 255, 128, 128, 128)]
        [DataRow("new(1.0f, 0.0f, 0.0f, 0.5f)", 128, 255, 0, 0)]
        public void CSharp_Float_Color_Values(string input, int expectedA, int expectedR, int expectedG, int expectedB)
        {
            Color actual = ColorUtils.HtmlToColor(input);
            Assert.AreEqual(expectedA, actual.A, $"Alpha mismatch for {input}");
            Assert.AreEqual(expectedR, actual.R, $"Red mismatch for {input}");
            Assert.AreEqual(expectedG, actual.G, $"Green mismatch for {input}");
            Assert.AreEqual(expectedB, actual.B, $"Blue mismatch for {input}");
        }

        [DataTestMethod]
        [DataRow("new Color(999, 0, 0)", 255, 255, 0, 0)]
        [DataRow("new Color(0, 0, 0, 999)", 255, 0, 0, 0)]
        public void CSharp_Int_Values_Should_Clamp(string input, int expectedA, int expectedR, int expectedG, int expectedB)
        {
            Color actual = ColorUtils.HtmlToColor(input);
            Assert.AreEqual(expectedA, actual.A, $"Alpha mismatch for {input}");
            Assert.AreEqual(expectedR, actual.R, $"Red mismatch for {input}");
            Assert.AreEqual(expectedG, actual.G, $"Green mismatch for {input}");
            Assert.AreEqual(expectedB, actual.B, $"Blue mismatch for {input}");
        }

        [TestMethod]
        public void CSharp_Code_With_Color_Should_Not_Conflict_With_Comment()
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor("var x = new Color(255, 0, 0); // red color");
            Assert.AreEqual(1, matches.Count());
        }

        [TestMethod]
        public void CSharp_Multiple_Colors_On_Same_Line()
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor("var a = new Color(255, 0, 0); var b = new Color(0, 255, 0);");
            Assert.AreEqual(2, matches.Count());
        }

        [TestMethod]
        public void CSharp_Color_Type_With_Custom_Helper()
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor("public static readonly Color PageBackground = Rgba(11, 15, 26);");
            Assert.AreEqual(1, matches.Count());
            Match match = matches.First();
            Color color = ColorUtils.HtmlToColor(match.Value);
            Assert.AreNotEqual(Color.Empty, color);
        }

        [TestMethod]
        public void CSharp_Method_Declaration_Should_Not_Match()
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor("private static Color Rgba(byte r, byte g, byte b, byte a = 255) => new(r, g, b, a);");
            Assert.AreEqual(0, matches.Count());
        }

        [DataTestMethod]
        [DataRow("new Color32(128, 255, 128, 255)")]
        [DataRow("new Color32(64, 128, 192, 255)")]
        [DataRow("Color color = new Color32(128, 255, 128, 255);")]
        [DataRow("Color32 winter = new Color32(246, 243, 237, 255);")]
        public void Unity_Color32_Should_Match(string input)
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor(input);
            Assert.IsTrue(matches.Count() >= 1, $"Expected at least 1 match for: {input}");
        }

        [DataTestMethod]
        [DataRow("new Color32(128, 255, 128, 255)", 255, 128, 255, 128)]
        [DataRow("new Color32(64, 128, 192, 255)", 255, 64, 128, 192)]
        [DataRow("new Color32(0, 0, 0, 0)", 0, 0, 0, 0)]
        public void Unity_Color32_Values(string input, int expectedA, int expectedR, int expectedG, int expectedB)
        {
            Color actual = ColorUtils.HtmlToColor(input);
            Assert.AreEqual(expectedA, actual.A, $"Alpha mismatch for {input}");
            Assert.AreEqual(expectedR, actual.R, $"Red mismatch for {input}");
            Assert.AreEqual(expectedG, actual.G, $"Green mismatch for {input}");
            Assert.AreEqual(expectedB, actual.B, $"Blue mismatch for {input}");
        }

        [TestMethod]
        public void Unity_Color_Float_Should_Match()
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor("new Color(0.2f, 1.0f, 0.7f, 0.8f)");
            Assert.AreEqual(1, matches.Count());
        }

        [TestMethod]
        public void Godot_Color_Float_Should_Match()
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor("new Color(0.2f, 1.0f, 0.7f)");
            Assert.AreEqual(1, matches.Count());
        }

        [TestMethod]
        public void Godot_Color8_Static_Should_Match()
        {
            IEnumerable<Match> matches = ColorUtils.MatchesColor("Color bg = Color.Color8(255, 0, 128, 255);");
            Assert.IsTrue(matches.Count() >= 1);
        }

        [TestMethod]
        public void Godot_Color8_Values()
        {
            Color actual = ColorUtils.HtmlToColor("Color8(255, 0, 128, 255)");
            Assert.AreEqual(255, actual.A);
            Assert.AreEqual(255, actual.R);
            Assert.AreEqual(0, actual.G);
            Assert.AreEqual(128, actual.B);
        }
    }
}
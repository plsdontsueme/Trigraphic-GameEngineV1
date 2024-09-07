using StbImageSharp;
using StbTrueTypeSharp;
using System.Collections.ObjectModel;

namespace Trigraphic_GameEngineV1
{
    internal sealed class Font : Texture
    {
        public static class Static
        {
            public static readonly Font ARIAL =
            CreateFont(
                "...//..//..//..//..//Rsc//Common//Fonts//arial.ttf",
                80,
                CharacterRange.BasicLatin, CharacterRange.Latin1Supplement
                );
        }
        public struct CharacterRange
        {
            //https://jrgraphix.net/r/Unicode/

            public static readonly CharacterRange BasicLatin = new CharacterRange(0x0020, 0x007F);
            public static readonly CharacterRange Latin1Supplement = new CharacterRange(0x00A0, 0x00FF);
            public static readonly CharacterRange LatinExtendedA = new CharacterRange(0x0100, 0x017F);
            public static readonly CharacterRange LatinExtendedB = new CharacterRange(0x0180, 0x024F);
            public static readonly CharacterRange Cyrillic = new CharacterRange(0x0400, 0x04FF);
            public static readonly CharacterRange CyrillicSupplement = new CharacterRange(0x0500, 0x052F);
            public static readonly CharacterRange Hiragana = new CharacterRange(0x3040, 0x309F);
            public static readonly CharacterRange Katakana = new CharacterRange(0x30A0, 0x30FF);
            public static readonly CharacterRange Greek = new CharacterRange(0x0370, 0x03FF);
            public static readonly CharacterRange CjkSymbolsAndPunctuation = new CharacterRange(0x3000, 0x303F);
            public static readonly CharacterRange CjkUnifiedIdeographs = new CharacterRange(0x4e00, 0x9fff);
            public static readonly CharacterRange HangulCompatibilityJamo = new CharacterRange(0x3130, 0x318f);
            public static readonly CharacterRange HangulSyllables = new CharacterRange(0xac00, 0xd7af);

            public int Start { get; }
            public int End { get; }
            public int Size => End - Start + 1;

            public CharacterRange(int start, int end)
            {
                Start = start;
                End = end;
            }
        }
        public struct GlyphInfo
        {
            public float X0, Y0, X1, Y1;
            public float Width, Height;
            public float XOffset, YOffset;
            public float XAdvance;
        }

        public readonly ReadOnlyDictionary<int, GlyphInfo> Glyphs;
        public readonly float LineHeight;


        private Font(float lineHeight, Dictionary<int, GlyphInfo> glyphs, ImageResult fontAtlas, bool smoothe) : base(fontAtlas, smoothe)
        {
            Glyphs = new(glyphs);
            LineHeight = lineHeight;
        }

        public static Font CreateFont(string path, float charResolutionV, params CharacterRange[] characterSet)
        {
            if (characterSet.Length == 0) throw new ArgumentException("no character set specified");

            byte[] ttf = File.ReadAllBytes(path);
            StbTrueType.stbtt_fontinfo fontInfo = StbTrueType.CreateFont(ttf, 0);

            if (fontInfo == null) throw new InvalidDataException("Failed to init font");

            var bitmapHeight = (int)(charResolutionV * 1.1f);
            var charCount = 0;
            foreach (var charRange in characterSet)
                charCount += charRange.Size;
            var bitmapWidth = charCount * bitmapHeight / 2;

            byte[] bitmap = new byte[bitmapWidth * bitmapHeight];
            StbTrueType.stbtt_pack_context context = new();
            unsafe
            {
                fixed (byte* pixelsPtr = bitmap)
                {
                    StbTrueType.stbtt_PackBegin(context, pixelsPtr, bitmapWidth, bitmapHeight, bitmapWidth, (int)(charResolutionV * .1f), null);
                }
            }

            var lineHeight = 1; // char height / charResolutionV

            Dictionary<int, GlyphInfo> glyphDict = new Dictionary<int, GlyphInfo>();
            foreach (var range in characterSet)
            {
                var charData = new StbTrueType.stbtt_packedchar[range.Size];
                unsafe
                {
                    fixed (StbTrueType.stbtt_packedchar* charDataPtr = charData)
                    {
                        StbTrueType.stbtt_PackFontRange(context, fontInfo.data, 0, charResolutionV, range.Start, range.Size, charDataPtr);
                    }
                }

                for (int i = 0; i < charData.Length; i++)
                {
                    var glyphInfo = new GlyphInfo
                    {
                        X0 = (float)charData[i].x0 / bitmapWidth,
                        Y0 = 1f - (float)charData[i].y1 / bitmapHeight,
                        X1 = (float)charData[i].x1 / bitmapWidth,
                        Y1 = 1f - (float)charData[i].y0 / bitmapHeight,
                        Width = (charData[i].x1 - charData[i].x0) / charResolutionV,
                        Height = (charData[i].y1 - charData[i].y0) / charResolutionV,
                        XOffset = charData[i].xoff / charResolutionV,
                        YOffset = charData[i].yoff / charResolutionV,
                        XAdvance = charData[i].xadvance / charResolutionV
                    };

                    glyphDict.Add(i + range.Start, glyphInfo);
                }
            }
            StbTrueType.stbtt_PackEnd(context);

            byte[] image = new byte[bitmap.Length * 4];
            int iy1 = bitmapHeight - 1;
            for (int iy = 0; iy < bitmapHeight; iy++)
            {
                for (int ix = 0; ix < bitmapWidth; ix++)
                {
                    int i = iy * bitmapWidth + ix;
                    int ind = (iy1 * bitmapWidth + ix) * 4;
                    image[ind] = 255;
                    image[ind + 1] = 255;
                    image[ind + 2] = 255;
                    image[ind + 3] = bitmap[i];
                }
                iy1--;
            }

            var atlasImage = new ImageResult() { Data = image, Width = bitmapWidth, Height = bitmapHeight };

            return new Font(lineHeight, glyphDict, atlasImage, charResolutionV > 25);
        }
    }
}

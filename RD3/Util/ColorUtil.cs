using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RD3
{
    public static class ColorUtil
    {
        /// <summary>
        /// 将Hex颜色码转换为System.Drawing.Color
        /// 支持格式：#RRGGBB、RRGGBB、#AARRGGBB、AARRGGBB
        /// </summary>
        /// <param name="hexCode">Hex颜色码</param>
        /// <returns>对应的System.Drawing.Color</returns>
        /// <exception cref="ArgumentException">Hex格式无效时抛出</exception>
        public static Color FromHexCode(string hexCode)
        {
            // 1. 预处理Hex码：移除#、转大写、清空空白字符
            string cleanHex = hexCode?.Trim().Replace("#", "").ToUpperInvariant() ?? string.Empty;

            // 2. 验证长度（仅支持6位RGB 或 8位AARRGGBB）
            if (cleanHex.Length != 6 && cleanHex.Length != 8)
            {
                throw new ArgumentException("Hex颜色码格式无效，需为#RRGGBB、RRGGBB、#AARRGGBB、AARRGGBB");
            }

            byte a = 255; // 默认不透明
            byte r, g, b;

            // 3. 解析逻辑分支
            if (cleanHex.Length == 6)
            {
                // 6位格式：RRGGBB（无Alpha）
                r = Convert.ToByte(cleanHex.Substring(0, 2), 16);
                g = Convert.ToByte(cleanHex.Substring(2, 2), 16);
                b = Convert.ToByte(cleanHex.Substring(4, 2), 16);
            }
            else
            {
                // 8位格式：AARRGGBB（前2位是Alpha，后6位是RGB）
                a = Convert.ToByte(cleanHex.Substring(0, 2), 16);
                r = Convert.ToByte(cleanHex.Substring(2, 2), 16);
                g = Convert.ToByte(cleanHex.Substring(4, 2), 16);
                b = Convert.ToByte(cleanHex.Substring(6, 2), 16);
            }

            // 4. 构造System.Drawing.Color（参数顺序：A, R, G, B）
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// 将System.Drawing.Color转为带#的6位RGB Hex码（无Alpha）
        /// 示例：Color.Red → "#FF0000"
        /// </summary>
        /// <param name="color">待转换的Color</param>
        /// <returns>6位Hex颜色码（带#）</returns>
        public static string ToHexRGB(this Color color)
        {
            // 格式化RGB为两位十六进制，补前导0（如0→00，F→0F）
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// 将System.Drawing.Color转为带#的8位RGBA Hex码（含Alpha）
        /// 示例：Color.FromArgb(128, 255, 0, 0) → "#80FF0000"
        /// </summary>
        /// <param name="color">待转换的Color</param>
        /// <returns>8位Hex颜色码（带#）</returns>
        public static string ToHexRGBA(this Color color)
        {
            // 先Alpha，后RGB，均为两位十六进制
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        // 随机数生成器（静态单例，避免重复初始化导致的伪随机）
        private static readonly Random _random = new Random();

        /// <summary>
        /// 生成随机颜色的ARGB十六进制码（#ARGB格式），排除白色系
        /// </summary>
        /// <param name="minA">Alpha通道最小值（默认0）</param>
        /// <param name="maxA">Alpha通道最大值（默认255）</param>
        /// <param name="minR">Red通道最小值（默认0）</param>
        /// <param name="maxR">Red通道最大值（默认255）</param>
        /// <param name="minG">Green通道最小值（默认0）</param>
        /// <param name="maxG">Green通道最大值（默认255）</param>
        /// <param name="minB">Blue通道最小值（默认0）</param>
        /// <param name="maxB">Blue通道最大值（默认255）</param>
        /// <returns>格式为#ARGB的十六进制颜色码（如#80FF5500）</returns>
        /// <exception cref="ArgumentOutOfRangeException">当最小值大于最大值时抛出</exception>
        public static string GetRandomColorHex(
            int minA = 0, int maxA = 255,
            int minR = 0, int maxR = 255,
            int minG = 0, int maxG = 255,
            int minB = 0, int maxB = 255)
        {
            // 验证参数范围（0-255且最小值≤最大值）
            ValidateChannelRange(minA, maxA, "Alpha");
            ValidateChannelRange(minR, maxR, "Red");
            ValidateChannelRange(minG, maxG, "Green");
            ValidateChannelRange(minB, maxB, "Blue");

            int a, r, g, b;
            // 循环生成，直到排除白色系（R/G/B均接近255的情况）
            do
            {
                // 生成各通道随机值（包含边界值）
                a = _random.Next(minA, maxA + 1);
                r = _random.Next(minR, maxR + 1);
                g = _random.Next(minG, maxG + 1);
                b = _random.Next(minB, maxB + 1);

                // 白色系判定：R/G/B均≥240（可根据需求调整阈值）
                // 阈值240是经验值，既排除纯白，也排除极浅的白色系（如浅灰）
            } while (r >= 240 && g >= 240 && b >= 240);

            // 格式化为#ARGB的十六进制字符串（两位大写，补前导0）
            return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// 验证颜色通道值的范围合法性
        /// </summary>
        private static void ValidateChannelRange(int min, int max, string channelName)
        {
            if (min < 0 || max > 255)
            {
                throw new ArgumentOutOfRangeException(
                    $"{channelName}通道值必须在0-255之间",
                    (min < 0 ? nameof(min) : nameof(max)));
            }
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(
                    $"{channelName}通道最小值({min})不能大于最大值({max})");
            }
        }
    }
}

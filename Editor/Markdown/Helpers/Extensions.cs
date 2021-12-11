using Markdig.Helpers;
using System.Runtime.CompilerServices;

namespace ThunderKit.Markdown.Helpers
{
    public static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountAndSkipChar(this StringSlice slice, char matchChar)
        {
            string text = slice.Text;
            int end = slice.End;
            int current = slice.Start;

            while (current <= end && (uint)current < (uint)text.Length && text[current] == matchChar)
            {
                current++;
            }

            int count = current - slice.Start;
            slice.Start = current;
            return count;
        }
    }
}
using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParserSites.Model
{
    /// <summary>
    /// Для сортировки полученных данных
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WordProperty<T>
    {
        public string Name { get; set; }

        public T Value { get; set; }

        public int Position { get; set; } = -1;
        public int EndPosition { get; set; } = -1;
        public int Depth { get; set; } = 0;
        public Thickness Margin => new Thickness(Depth * 16, 1, 0, 1);
    }

    public class Tag<T> : WordProperty<T>
    {
        public List<Atribute> Atribute { get; set; } = new List<Atribute>();
        public bool IsSelfClosing { get; set; }
    }

    public class Atribute : WordProperty<string>
    {

    }

    public class Word : WordProperty<string>
    {

    }
}

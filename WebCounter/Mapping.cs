using System;

namespace WebCounter
{
    public class Mapping : Attribute
    {
        public string Map;
        public Mapping(string map)
            => Map = map;
    }
}

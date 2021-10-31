using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project11
{
    public class Book
    {
        public string _name { get; set; }
       public string _path { get; set; }
        public bool _favorites { get; set; }

        public double _size { get; set; }
        public Book()
        {

        }

        public Book(string name, bool favorite, string path, double size)
        {
            _name = name;          
            _favorites = favorite;
            _path = path;
            _size = size;
        }

        public override string ToString()
        {
            return $"Name {_name}";
        }

        public override bool Equals(object obj)
        {

            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Book book = (Book)obj;
                return (_size == book._size) && (_path == book._path);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}

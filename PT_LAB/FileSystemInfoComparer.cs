using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT_LAB
{
    public class FileSystemInfoComparer : IComparer<FileSystemInfoViewModel>
    {
        private readonly SortOptions.SortByOption _sortBy;
        private readonly SortOptions.SortDirectionOption _direction;
        public List<string> ComparisonLog { get; private set; } = new List<string>();

        public FileSystemInfoComparer(SortOptions.SortByOption sortBy, SortOptions.SortDirectionOption direction)
        {
            _sortBy = sortBy;
            _direction = direction;
        }

        public int Compare(FileSystemInfoViewModel x, FileSystemInfoViewModel y)
        {
            int result = 0;

            switch (_sortBy)
            {
                case SortOptions.SortByOption.Name:
                    result = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                    break;
                case SortOptions.SortByOption.Extension:
                    if (x is FileInfoViewModel xFile && y is FileInfoViewModel yFile)
                        result = string.Compare(xFile.Extension, yFile.Extension, StringComparison.OrdinalIgnoreCase);
                    break;
                case SortOptions.SortByOption.Size:
                    if (x is FileInfoViewModel xFile1 && y is FileInfoViewModel yFile1)
                        result = xFile1.Size.CompareTo(yFile1.Size);
                    break;
                case SortOptions.SortByOption.Date:
                    result = x.Model.LastWriteTime.CompareTo(y.Model.LastWriteTime);
                    break;
            }

            if (_direction == SortOptions.SortDirectionOption.Descending)
            { result = -result; }

            string comparisonResult = $"Comparing {x.Name} and {y.Name}: {result}";
            ComparisonLog.Add(comparisonResult);

            return result;
        }
    }

}

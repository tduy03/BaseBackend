using System;
using VDCD.Entities.Enums;

namespace VDCD.Entities.DTO.Request
{
    public class UserFilterRequest
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value > 0 ? value : 1;
        }

        public int? Page
        {
            get => _pageNumber;
            set
            {
                if (value.HasValue && value.Value > 0)
                    _pageNumber = value.Value;
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 0 ? (value > 100 ? 100 : value) : 10;
        }

        public string? Keyword { get; set; }

        public string? Search
        {
            get => Keyword;
            set => Keyword = value;
        }

        public UserStatus? Status { get; set; }

        public string? RoleCode { get; set; }

        public string? SortBy { get; set; } = "CreatedAt";

        public bool IsDescending { get; set; } = true;
    }
}

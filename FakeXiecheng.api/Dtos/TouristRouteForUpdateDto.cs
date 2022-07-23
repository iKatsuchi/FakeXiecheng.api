using FakeXiecheng.api.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FakeXiecheng.api.Dtos
{
    public class TouristRouteForUpdateDto:TouristRouteForMainDto
    {
        [Required(ErrorMessage ="更新必备")]
        [MaxLength(1500)]
        public override string Description { get; set; }
    }
}

using FakeXiecheng.api.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FakeXiecheng.api.Dtos
{
    public class TouristRouteForCreateionDto :TouristRouteForMainDto //: IValidatableObject
    {
        //已经在ValidationAttibute文件中引用了 所以这个不用了
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(Title == Description)
        //    {
        //        yield return new ValidationResult(
        //            "路线名称必须与路线描述不同",
        //            new[] { "TouristRouteForCreationDto" }
        //            );
        //    }
        //}
    }
}

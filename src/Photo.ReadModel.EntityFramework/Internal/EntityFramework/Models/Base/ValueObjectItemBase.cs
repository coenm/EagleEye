﻿namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models.Base
{
    using System;
    using System.ComponentModel.DataAnnotations;

    internal abstract class ValueObjectItemBase
    {
        [Key]
        public Guid Id { get; set; }
    }
}

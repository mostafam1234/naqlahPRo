﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AssistanWork
    {
        public AssistanWork()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
        }
        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public string EnglishName { get; private set; }
        public bool IsDeleted { get;private set; }

    }
}

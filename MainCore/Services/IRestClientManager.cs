﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainCore.Services
{
    public interface IRestClientManager
    {
        public RestClient Get(int id);
    }
}
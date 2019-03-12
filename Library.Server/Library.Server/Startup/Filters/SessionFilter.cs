﻿using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Library.Server.Startup.Filters
{
    public class SessionFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "X-Session",
                In = "header",
                Type = "string",
                Required = false
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "sessionId",
                In = "query",
                Type = "string",
                Required = false
            });
        }
    }
}
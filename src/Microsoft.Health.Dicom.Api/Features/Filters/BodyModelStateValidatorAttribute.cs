﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using EnsureThat;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Health.Dicom.Api.Web;

namespace Microsoft.Health.Dicom.Api.Features.Filters
{
    public sealed class BodyModelStateValidatorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            if (!context.ModelState.IsValid)
            {
                throw new InvalidRequestBodyException(Environment.NewLine + string.Join(Environment.NewLine, context.ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage)));
            }
        }
    }
}

using Microsoft.AspNetCore.Rewrite;
using System.Text.RegularExpressions;

namespace IRB.RevigoWeb
{
    public class RewriteAspxUrl : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;

            if (request.Path.Value.EndsWith(".aspx"))
            {
                request.Path = request.Path.Value.Substring(0, request.Path.Value.Length - 5);
                request.HttpContext.Response.Redirect(request.Path, true);
            }

            context.Result = RuleResult.ContinueRules;
        }
    }
}

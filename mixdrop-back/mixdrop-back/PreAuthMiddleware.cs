namespace mixdrop_back
{
    public class PreAuthMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            bool canContinue = true;

            if (context.Request.Path.Value.Contains("socket"))
            {
                canContinue = false;

                // Coge el JWT desde la ruta de la api
                var routeDict = context.Request.Query.FirstOrDefault();
                if (routeDict.Key != null && routeDict.Key.Equals("jwt"))
                {
                    var jwt = routeDict.Value;
                    //context.Request.Headers.Append("Authorization", "Bearer " + jwt);
                    // TODO: Verificar JWT
                    canContinue = true;
                }
            }

            if(canContinue) await next(context);
        }
    }
}

namespace mixdrop_back
{
    public class PreAuthMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string pathValue = context.Request.Path.Value;

            if (pathValue.Contains("socket"))
            {
                int index = pathValue.IndexOf("/", 2); // En la segunda barra se encontraría el JWT (espero xD)
                string jwt = pathValue.Substring(index + 1);
                context.Request.Headers.Append("Authorization", "Bearer " + jwt);
            }

            await next(context);
        }
    }
}

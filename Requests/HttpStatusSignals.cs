namespace uwap.WebFramework;

public class HttpStatusSignal(int status) : Exception
{
    public readonly int Status = status;
}

public class NotChangedSignal() : HttpStatusSignal(304);

public class BadRequestSignal() : HttpStatusSignal(400);

public class NotAuthenticatedSignal() : HttpStatusSignal(401);

public class ForbiddenSignal() : HttpStatusSignal(403);

public class NotFoundSignal() : HttpStatusSignal(404);

public class BadMethodSignal() : HttpStatusSignal(405);

public class PayloadTooLargeSignal() : HttpStatusSignal(413);

public class TeapotSignal() : HttpStatusSignal(418);

public class TooManyRequestsSignal() : HttpStatusSignal(429);

public class ServerErrorSignal() : HttpStatusSignal(500);

public class NotImplementedSignal() : HttpStatusSignal(501);

public class InsufficientStorageSignal() : HttpStatusSignal(507);
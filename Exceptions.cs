using System;

public class EmptyUrl : Exception
{
    public EmptyUrl() : base(String.Format("URI cannot be empty."))
    {
    }
}

public class EmptyResponse : Exception
{
    public EmptyResponse() : base(String.Format("Response is empty."))
    {
    }
}

public class OptionNotFound : Exception
{
    public OptionNotFound() : base(String.Format("Option Not Found."))
    {
    }
}


public class KeyNotFound : Exception
{
    public KeyNotFound() : base(String.Format("Key Not Found."))
    {
    }
}

public class InvalidRequest : Exception
{
    public InvalidRequest() : base(String.Format("Invalid request."))
    {
    }
}

public class ErrorOccurred : Exception
{
    public ErrorOccurred() : base(String.Format("Error Occurred."))
    {
    }
}
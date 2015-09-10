﻿namespace CentauriTac.Messaging
{
    public interface IHandler<in T> where T: IMessage
    {
        void Handle(T args);
    } 
}

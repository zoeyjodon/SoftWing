/*****************************************************************//**
 * \file    SystemSubscriber.cs
 * \brief   Abstract class definition for subscribers to system-level messages
 *
 * \author Lucas Jodon 
 * \date   8/14/2020
***********************************************************************/
namespace SoftWing.System
{
    public interface MessageSubscriber
    {
        void Accept(SystemMessage message);
    }

}

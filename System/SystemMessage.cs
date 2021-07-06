/*****************************************************************//**
 * \file    SystemMessage.cs
 * \brief   Interface definition for system-level messages
 *
 * \author Lucas Jodon 
 * \date   8/14/2020
***********************************************************************/
namespace SoftWing.System
{
    public enum MessageType : byte
    {
        Invalid,
        AudioUpdate,
        DisplayUpdate,
        ControlUpdate,
        ShowIme,
    }

    public interface SystemMessage
    {
        MessageType getMessageType();
    }
}

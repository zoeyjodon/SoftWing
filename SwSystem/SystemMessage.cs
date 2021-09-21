/*****************************************************************//**
 * \file    SystemMessage.cs
 * \brief   Interface definition for system-level messages
 *
 * \author Lucas Jodon 
 * \date   8/14/2020
***********************************************************************/
namespace SoftWing.SwSystem
{
    public enum MessageType : byte
    {
        Invalid,
        AudioUpdate,
        DisplayUpdate,
        ControlUpdate,
        ShowIme,
        DonationUpdate,
    }

    public interface SystemMessage
    {
        MessageType getMessageType();
    }
}

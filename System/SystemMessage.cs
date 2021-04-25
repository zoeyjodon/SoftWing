﻿/*****************************************************************//**
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
        DisplayUpdate,
        ControlUpdate,
    }

    public interface SystemMessage
    {
        MessageType getMessageType();
    }
}
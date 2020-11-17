﻿using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DisruptorBeam;

public class OfflineNotificationBehaviour : MonoBehaviour
{
    public GameObject notification;
    public GameObject message;
    public GameObject wifiLostIcon;
    public LayoutElement offlineObjectLayout;
    private DisruptorBeam.IDisruptorEngine _engineInstance;    

    private async void Start()
    {
        _engineInstance = await DisruptorBeam.DisruptorEngine.Instance;
        _engineInstance.ConnectivityService.OnConnectivityChanged += toggleOfflineNotification;
        if(!_engineInstance.ConnectivityService.HasConnectivity)
        {
            toggleOfflineNotification(false);
        }
    }

    public void toggleOfflineNotification(bool OfflineStatus)
    {
        if (!OfflineStatus)
        {
            offlineObjectLayout.preferredHeight = 90;
        }
        else
        {
            offlineObjectLayout.preferredHeight = 0;
        }
        notification.SetActive(!OfflineStatus);
        message.SetActive(!OfflineStatus);
        wifiLostIcon.SetActive(!OfflineStatus);
    }
}
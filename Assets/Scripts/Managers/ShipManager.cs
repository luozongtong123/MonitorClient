﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipManager : SingletonUnity<ShipManager>
{
    public GameObject pf_Ship;
    public int RequestFPS = 5;

    private GameObject m_World;

    private GetParamApi m_GetParamApi;
	private Dictionary<int, GameObject> m_ShipDict;

	private int m_ControlShipID = 1;
    private int m_iInstanceID = 0;
    private float m_fUpdateTime = 0;

	public int ControlShipID
	{
		get{return this.m_ControlShipID;}
		set{this.m_ControlShipID = value;}
	}

    void Awake()
    {
        this.m_ShipDict = new Dictionary<int, GameObject> ();
        this.m_GetParamApi = new GetParamApi ();
        this.m_GetParamApi.AddCallback (this.SetShipParam);
    }

	// Use this for initialization
	void Start ()
	{
        this.m_World = GameObject.Find ("World");
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (this.m_fUpdateTime >= 1f / (float)RequestFPS)
        {
            if (GlobalManager.Instance.IsGameRunning && this.m_GetParamApi.IsIdle())
            {
                StartCoroutine (this.m_GetParamApi.Request (this.m_iInstanceID));
                this.m_fUpdateTime = 0;
            }
        }

        this.m_fUpdateTime += 1;
	}

    /// <summary>
    /// 创建新实例
    /// </summary>
    /// <param name="iInstanceID">实例ID</param>
    /// <param name="oInstance">实例数据</param>
    public void CreateNewInstance(int iInstanceID, InstanceResp oInstance)
    {
        foreach(string sShipID in oInstance.shape.Keys)
        {
            CreateNewShip (int.Parse(sShipID));
        }

        this.m_iInstanceID = iInstanceID;
        GlobalManager.Instance.IsGameRunning = true;

        if (this.m_ShipDict.Count > 0)
        {
            CameraController.Instance.LookAtObject = this.m_ShipDict [1];
        }
    }

    /// <summary>
    /// 创建新录像实例
    /// </summary>
    /// <param name="iInstanceID">实例ID</param>
    /// <param name="oData">录像数据</param>
    public void CreateNewVideoInstance(int iInstanceID, int iShipNum)
    {
        for(int i=1; i <= iShipNum; i++)
        {
            CreateNewShip (i);
        }

        this.m_iInstanceID = iInstanceID;
        GlobalManager.Instance.IsGameRunning = false;

        if (this.m_ShipDict.Count > 0)
        {
            CameraController.Instance.LookAtObject = this.m_ShipDict [1];
        }
    }

    /// <summary>
    /// Creates the new ship.
    /// </summary>
    /// <param name="iShipID">ship ID.</param>
    public void CreateNewShip(int iShipID)
    {
        GameObject oShip = Instantiate (this.pf_Ship) as GameObject;
        oShip.transform.parent = this.m_World.transform;
        this.m_ShipDict.Add (iShipID, oShip);
    }

    /// <summary>
    /// 设置船舶参数
    /// </summary>
    /// <param name="oSender">sender.</param>
    /// <param name="oShipParam">ship parameter.</param>
    public void SetShipParam(object oSender, object oShipParam)
    {
        if(!GlobalManager.Instance.IsGameRunning && !GlobalManager.Instance.IsVideoRunning)
        {
            return;
        }
        Dictionary<int, SShipParam> dShipParam = oShipParam as Dictionary<int, SShipParam>;
        foreach (var item in dShipParam) 
        {
            ShipModel oModel = this.m_ShipDict [item.Key].GetComponent<ShipModel> ();
            if (item.Value != null) 
            {
                oModel.Param = item.Value;
                SignalManager.Instance.DispatchSignal (SignalID.ShipParamChanged, item.Key, item.Value);
            }
        }

    }

	/// <summary>
	/// 销毁所有船只
	/// </summary>
	public void DestroyShip()
	{
		foreach(var item in this.m_ShipDict)
		{
			Destroy (item.Value);
		}
		this.m_ShipDict.Clear ();
		this.m_ControlShipID = 1;
	}

    public GameObject GetShipObjectByID(int iShipID)
    {
        return this.m_ShipDict [iShipID];
    }

	public int GetShipAmount()
	{
		return this.m_ShipDict.Count;
	}
}


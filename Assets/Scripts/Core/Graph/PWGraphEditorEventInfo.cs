﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using PW.Core;
using PW.Node;
using System.Reflection;

public class PWGraphEditorEventInfo : IPWCloneable< PWGraphEditorEventInfo > {

	//graph infos:
	public bool				isMouseClickOutside;
	public bool				isSelecting;
	public Vector2			selectionStartPoint;
	public bool				isDraggingSelectedNodes;
	public int				selectedNodeCount;
	public bool				isPanning;
	public bool				isDraggingNewLink;
	public PWAnchor			startedLinkAnchor;

	//node infos:
	public bool				isMouseOverNode { get { return mouseOverNode != null; } }
	public bool				isDraggingNode;
	public bool				isMouseClickOnNode;
	public PWNode			mouseOverNode;

	//link infos:
	public bool				isMouseOverLink { get { return mouseOverLink != null; } }
	public bool				isMouseClickOnLink;
	public bool				isDraggingLink;
	public PWNodeLink		mouseOverLink;

	//anchor infos:
	public bool				isMouseOverAnchor { get { return mouseOverAnchor != null; } }
	public bool				isMouseClickOnAnchor;
	public PWAnchor			mouseOverAnchor;

	//ordering group infos:
	public bool				isMouseOverOrderingGroup { get { return mouseOverOrderingGroup != null; } }
	public bool				isMouseClickOnOrderingGroup;	//not implemented
	public bool				isDraggingOrderingGroup;		//not implemented
	public bool				isResizingOrderingGroup;		//not implemented
	public PWOrderingGroup	mouseOverOrderingGroup;

    public bool isDraggingSomething { get { return isDraggingLink || isDraggingNewLink || isDraggingNode || isDraggingOrderingGroup || isDraggingSelectedNodes; } }
    public bool isMouseOverSomething { get { return isMouseOverAnchor || isMouseOverLink || isMouseOverNode || isMouseOverOrderingGroup; } }

	static PropertyInfo[] propList;

	public PWGraphEditorEventInfo()
	{
		//initialize self proper list for Clone method
		if (propList == null)
			propList = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
		
		Init();
	}

	//init defaut values
	void Init()
	{
		isMouseClickOutside = false;
		isSelecting = false;
		selectionStartPoint = Vector2.zero;
		isDraggingSelectedNodes = false;
		isPanning = false;
		isDraggingNewLink = false;
		startedLinkAnchor = null;
		selectedNodeCount = 0;

		isMouseClickOnNode = false;
		isDraggingNode = false;
		mouseOverNode = null;

		isMouseClickOnLink = false;
		isDraggingLink = false;
		mouseOverLink = null;

		isMouseClickOnAnchor = false;
		mouseOverAnchor = null;

		isMouseClickOnOrderingGroup = false;
		isDraggingOrderingGroup = false;
		isResizingOrderingGroup = false;
		mouseOverOrderingGroup = null;
	}

	//events that will be reset each frames
	public void Reset()
	{
		isMouseClickOutside = false;
		isPanning = false;
		
		isMouseClickOnNode = false;
		mouseOverNode = null;
		
		isMouseClickOnLink = false;
		mouseOverLink = null;

		isMouseClickOnAnchor = false;
		mouseOverAnchor = null;
		
		isMouseClickOnOrderingGroup = false;
		mouseOverOrderingGroup = null;
	}

	public PWGraphEditorEventInfo Clone(PWGraphEditorEventInfo oldObject = null)
	{
		if (oldObject == null)
			oldObject = new PWGraphEditorEventInfo();
		
		foreach (var prop in propList)
			prop.SetValue(oldObject, prop.GetValue(this, null), null);
		
		return oldObject;
	}
}

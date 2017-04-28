﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace PW
{
	[System.SerializableAttribute]
	public class PWOrderingGroup {
	
		public Rect					orderGroupRect;
		public string				name;
		public SerializableColor	color;

		[System.NonSerializedAttribute]
		public bool					resizing = false;

		int							callbackId;
		int							resizingCallbackId;

		GUIStyle					orderingGroupStyle;
		GUIStyle					movepadStyle;
		Texture2D					ic_edit;

		public PWOrderingGroup(Vector2 pos)
		{
			orderGroupRect = new Rect();
			orderGroupRect.position = pos;
			orderGroupRect.size = new Vector2(240, 120);
			name = "ordering group";
			color = (SerializableColor)Color.white;
		}

		void CreateAnchorRectCallabck(Rect r, MouseCursor cursor, Action callback)
		{
			EditorGUIUtility.AddCursorRect(r, cursor);

			if (resizing && callbackId == resizingCallbackId && Event.current.type == EventType.mouseDrag)
				callback();
			if (r.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
				{
					resizing = true;
					resizingCallbackId = callbackId;
					Event.current.type = EventType.Used;
				}
			}
			callbackId++;
		}

		void LoadStyles()
		{
			orderingGroupStyle = GUI.skin.FindStyle("OrderingGroup");
			movepadStyle = GUI.skin.FindStyle("Movepad");
			ic_edit = Resources.Load("ic_edit") as Texture2D;
		}

		public bool Render(Vector2 graphDecal, Vector2 screenSize)
		{
			var e = Event.current;
			Rect screen = new Rect(-graphDecal, screenSize);

			//check if ordering group is not visible
			if (!orderGroupRect.Overlaps(screen))
				return false;
			
			//TODO: remove: only for testing
			// if (orderingGroupStyle == null)
				LoadStyles();

			Rect		orderGroupWorldRect = PWUtils.DecalRect(orderGroupRect, graphDecal);

			callbackId = 0;

			int			controlSize = 8;
			int			cornerSize = 14;

			CreateAnchorRectCallabck( //left resize anchor
				new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y + cornerSize, controlSize, orderGroupWorldRect.height - cornerSize * 2),
				MouseCursor.ResizeHorizontal,
				() => orderGroupRect.xMin += e.delta.x
			);
			CreateAnchorRectCallabck( //right resize anchor
				new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - controlSize, orderGroupWorldRect.y + cornerSize, controlSize, orderGroupWorldRect.height - cornerSize * 2),
				MouseCursor.ResizeHorizontal,
				() => orderGroupRect.xMax += e.delta.x
			);
			CreateAnchorRectCallabck( //top resize anchor
				new Rect(orderGroupWorldRect.x + cornerSize, orderGroupWorldRect.y, orderGroupWorldRect.width - cornerSize * 2, controlSize),
				MouseCursor.ResizeVertical,
				() => orderGroupRect.yMin += e.delta.y
			);
			CreateAnchorRectCallabck( //down resize anchor
				new Rect(orderGroupWorldRect.x + cornerSize, orderGroupWorldRect.y + orderGroupWorldRect.height - controlSize, orderGroupWorldRect.width - cornerSize * 2, controlSize),
				MouseCursor.ResizeVertical,
				() => orderGroupRect.yMax += e.delta.y
			);

			CreateAnchorRectCallabck( //top left anchor
				new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y, cornerSize, cornerSize),
				MouseCursor.ResizeUpLeft,
				() => {orderGroupRect.yMin += e.delta.y; orderGroupRect.xMin += e.delta.x;}
			);
			CreateAnchorRectCallabck( //top right anchor
				new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - cornerSize, orderGroupWorldRect.y, cornerSize, cornerSize),
				MouseCursor.ResizeUpRight,
				() => {orderGroupRect.yMin += e.delta.y; orderGroupRect.xMax += e.delta.x;}
			);
			CreateAnchorRectCallabck( //down left anchor
				new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y + orderGroupWorldRect.height - cornerSize, cornerSize, cornerSize),
				MouseCursor.ResizeUpRight,
				() => {orderGroupRect.yMax += e.delta.y; orderGroupRect.xMin += e.delta.x;}
			);
			CreateAnchorRectCallabck( //down right anchor
				new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - cornerSize, orderGroupWorldRect.y + orderGroupWorldRect.height - cornerSize, cornerSize, cornerSize),
				MouseCursor.ResizeUpLeft,
				() => {orderGroupRect.yMax += e.delta.y; orderGroupRect.xMax += e.delta.x;}
			);

			if (e.type == EventType.MouseUp)
				resizing = false;

			Rect nameRect = orderGroupWorldRect;
			nameRect.yMin -= 10;
			GUI.Label(nameRect, name);
			Rect editNameRect = new Rect(nameRect.position + Vector2.right * 20, Vector2.one * 16);
			GUI.DrawTexture(editNameRect, ic_edit);

			GUI.color = color;
			GUI.Label(orderGroupWorldRect, (string)null, orderingGroupStyle);
			GUI.color = Color.white;

			return (orderGroupWorldRect.Contains(e.mousePosition));
		}

	}
}
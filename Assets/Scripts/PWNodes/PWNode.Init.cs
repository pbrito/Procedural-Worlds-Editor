﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;
using PW.Core;

using PW.Node;

//Initialization and data baking for PWNode
namespace PW
{
	public partial class PWNode
	{
		static GUIStyle 			renameNodeTextFieldStyle = null;
		static GUIStyle				debugStyle = null;
		static GUIStyle				innerNodePaddingStyle = null;
		static GUIStyle				nodeStyle = null;
		static bool					styleLoaded = false;
		
		static Texture2D			editIcon = null;
		static Texture2D			debugIcon = null;

		//anchor 
		[System.Serializable]
		public class AnchorFieldDictionary : SerializableDictionary< string, PWAnchorField > {}
		[SerializeField]
		protected AnchorFieldDictionary		anchorFieldDictionary = new AnchorFieldDictionary();

		void LoadAssets()
		{
			editIcon = Resources.Load< Texture2D >("ic_edit");
			debugIcon = Resources.Load< Texture2D >("ic_settings");
			
			//set the color scheme name for this node type
			colorSchemeName = PWNodeTypeProvider.GetNodeColor(GetType());
		}

		public void LoadStyles()
		{
			//check if style was already initialized:
			if (innerNodePaddingStyle != null)
				return ;

			renameNodeTextFieldStyle = GUI.skin.FindStyle("RenameNodetextField");
			innerNodePaddingStyle = GUI.skin.FindStyle("WindowInnerPadding");
			debugStyle = GUI.skin.FindStyle("Debug");
			nodeStyle = GUI.skin.FindStyle("Node");

			styleLoaded = true;
		}
		
		void LoadFieldAttributes()
		{
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			List< string > actualFields = new List< string >();
			foreach (var field in fInfos)
			{
				// reading field informations.
				actualFields.Add(field.Name);
				if (!anchorFieldDictionary.ContainsKey(field.Name))
					anchorFieldDictionary[field.Name] = new PWAnchorField();
				
				PWAnchorField	anchorField = anchorFieldDictionary[field.Name];
				
				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					PWInputAttribute		inputAttr = attr as PWInputAttribute;
					PWOutputAttribute		outputAttr = attr as PWOutputAttribute;
					PWColorAttribute		colorAttr = attr as PWColorAttribute;
					PWOffsetAttribute		offsetAttr = attr as PWOffsetAttribute;
					PWMultipleAttribute		multipleAttr = attr as PWMultipleAttribute;
					PWGenericAttribute		genericAttr = attr as PWGenericAttribute;
					PWNotRequiredAttribute	notRequiredAttr = attr as PWNotRequiredAttribute;
					PWCopyAttribute			copyAttr = attr as PWCopyAttribute;

					if (inputAttr != null)
					{
						anchorField.anchorType = PWAnchorType.Input;
						if (inputAttr.name != null)
							anchorField.name = inputAttr.name;
					}
					if (outputAttr != null)
					{
						anchorField.anchorType = PWAnchorType.Output;
						if (outputAttr.name != null)
							anchorField.name = outputAttr.name;
					}
					if (copyAttr != null)
						anchorField.transferType = PWTransferType.Copy;
					if (colorAttr != null)
						anchorField.color = new SerializableColor(colorAttr.color);
					if (offsetAttr != null)
					{
						anchorField.offset = offsetAttr.offset;
						anchorField.padding = offsetAttr.padding;
					}
					if (multipleAttr != null)
					{
						//check if field is PWValues type otherwise do not implement multi-anchor
						anchorField.allowedTypes = multipleAttr.allowedTypes;
						anchorField.minMultipleValues = multipleAttr.minValues;
						anchorField.maxMultipleValues = multipleAttr.maxValues;
					}
					if (genericAttr != null)
						anchorField.allowedTypes = genericAttr.allowedTypes;
					if (notRequiredAttr != null)
						anchorField.required = false;
				}
				if (anchorField.anchorType == PWAnchorType.None) //field does not have a PW attribute
					anchorFieldDictionary.Remove(field.Name);
				else
				{
					anchorField.colorSchemeName = colorSchemeName;
					anchorField.CreateNewAnchor();
					anchorField.fieldName = field.Name;
					anchorField.fieldType = (SerializableType)field.FieldType;
					anchorField.nodeRef = this;
				}
			}

			//remove inhexistants field dictionary entries (for renamed variables):
			var toRemoveKeys = new List< string >();
			foreach (var kp in anchorFieldDictionary)
				if (!actualFields.Contains(kp.Key))
					toRemoveKeys.Add(kp.Key);
			foreach (var toRemoveKey in toRemoveKeys)
				anchorFieldDictionary.Remove(toRemoveKey);
		}

		void UpdateAnchorProperties()
		{
			//we check if an anchor have been update (field name changed)
			// and then remove it from the anchor list and take the new field attributes
			// we do the same for every properties
			foreach (var anchorFieldKP in anchorFieldDictionary)
			{
				var af = anchorFieldKP.Value;

				foreach (var anchor in anchorFields)
				{
					//we find the field in inputAnchors corresponding to af
					if (anchor.anchorType == af.anchorType && anchor.fieldName == af.fieldName)
					{
						//we update the values we have in our input/output anchorFileds with the new fresh field values
						anchor.name = af.name;
						anchor.fieldType = af.fieldType;
						anchor.debug = af.debug;
						anchor.transferType = af.transferType;
						anchor.offset = af.offset;
						anchor.padding = af.padding;
						anchor.colorSchemeName = af.colorSchemeName;
						anchor.color = af.color;
						anchor.allowedTypes = af.allowedTypes;
						anchor.minMultipleValues = af.minMultipleValues;
						anchor.maxMultipleValues = af.maxMultipleValues;
						anchor.required = af.required;

						//we tell that the anchor have been linked to an actual field
						anchor.fieldValidated = true;
						af.fieldValidated = true;
					}
				}
			}
			
			//we remove every anchors that is not anymore linked with a field
			inputAnchorFields.RemoveAll(a => !a.fieldValidated);
			outputAnchorFields.RemoveAll(a => !a.fieldValidated);

			//we add new fields to the input/output anchors list
			foreach (var anchorFieldKP in anchorFieldDictionary)
			{
				var af = anchorFieldKP.Value;
				//if the field was not found in both input and output anchor lists
				if (!af.fieldValidated)
				{
					if (af.anchorType == PWAnchorType.Input)
						inputAnchorFields.Add(af);
					else
						outputAnchorFields.Add(af);
				}
			}
		}

		//retarget "Reload" button in the editor to the internal event OnReload:
		void GraphReloadCallback() { Reload(null); }

		void ForceReloadCallback() { Reload(null); }

		void ForceReloadOnceCallback() { /*TODO*/ }

		void LinkDraggedCallback(PWAnchor fromAnchor)
		{
			//disable non-linkable anchors:
			if (fromAnchor.nodeRef != this)
				DisableUnlinkableAnchors(fromAnchor);
		}

		void LinkCanceled()
		{
			//reset the highlight mode on anchors:
			ResetUnlinkableAnchors();
		}

		void DraggedLinkOverAnchorCallback(PWAnchor anchor)
		{
			if (anchor.anchorType == PWAnchorType.Input)
			{
				if (anchor.linkCount == 1)
					anchor.highlighMode = PWAnchorHighlight.AttachReplace;
				else
					anchor.highlighMode = PWAnchorHighlight.AttachNew;
			}
			else
			{
				if (anchor.linkCount > 0)
					anchor.highlighMode = PWAnchorHighlight.AttachAdd;
				else
					anchor.highlighMode = PWAnchorHighlight.AttachNew;
			}
			
			//TODO: snap the dragged link
		}

		void DraggedLinkQuitAnchorCallbck(PWAnchor anchor)
		{
			//TODO: update the anchor highlight
			anchor.highlighMode = PWAnchorHighlight.None;
		}

		void AnchorLinkedCallback(PWAnchor anchor)
		{
			//CreateLink will raise the OnLinkCreated event in the graph and create the link
			PWNodeLink link = graphRef.CreateLink(anchor);

			//update canWork bool
			UpdateWorkStatus();
		}

		void AnchorUnlinkedCallback(PWAnchor anchor)
		{
			//TODO: unlink the anchor, remove the link.

			//raise internal event 
			// OnLinkRemoved(link);
		}

		void NodeSelectedCallback(PWNode node)
		{
			if (node == this)
				isSelected = true;
		}

		void NodeUnselectedCallback(PWNode node)
		{
			if (node == this)
				isSelected = false;
		}

		void LinkRemovedCalllback(PWNodeLink link)
		{
		}

		void LinkCreatedCallback(PWNodeLink link)
		{
			if (link.fromNode == this)
				link.fromAnchor.AddLink(link);
			else if (link.toNode == this)
				link.toAnchor.AddLink(link);
		}

		void NodeRemovedCallback(PWNode node)
		{
			if (node == this)
				RemoveSelf();
		}

		void BindEvents()
		{
			//graph events:
			//if the node is used in a PWMainGraph:
			graphRef.OnForceReload += ForceReloadCallback;
			graphRef.OnForceReloadOnce += ForceReloadOnceCallback;
			graphRef.OnClickNowhere += OnClickedOutside;
			graphRef.OnLinkDragged += LinkDraggedCallback;
			// graphRef.OnLinkRemoved += LinkRemovedCalllback; //unused
 			graphRef.OnLinkCreated += LinkCreatedCallback;
			graphRef.OnLinkCanceled += LinkCanceled;
			graphRef.OnNodeSelected += NodeSelectedCallback;
			graphRef.OnNodeUnselected += NodeUnselectedCallback;
			graphRef.OnNodeRemoved += NodeRemovedCallback;

			//local node events:
			OnDraggedLinkOverAnchor += DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor += DraggedLinkQuitAnchorCallbck;
			OnAnchorLinked += AnchorLinkedCallback;
			OnAnchorUnlinked += AnchorUnlinkedCallback;
		}

		void UnBindEvents()
		{
			//if the node is used in a PWMainGraph:
			if (graphRef != null)
			{
				graphRef.OnForceReload -= GraphReloadCallback;
				graphRef.OnForceReloadOnce -= ForceReloadOnceCallback;
				graphRef.OnClickNowhere -= OnClickedOutside;
				graphRef.OnLinkDragged -= LinkDraggedCallback;
				// graphRef.OnLinkRemoved -= LinkRemovedCalllback; //unused
				graphRef.OnLinkCreated -= LinkCreatedCallback;
				graphRef.OnLinkCanceled -= LinkCanceled;
				graphRef.OnNodeSelected -= NodeSelectedCallback;
				graphRef.OnNodeUnselected -= NodeUnselectedCallback;
				graphRef.OnNodeRemoved -= NodeRemovedCallback;
			}
			
			//local node events:
			OnDraggedLinkOverAnchor -= DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor -= DraggedLinkQuitAnchorCallbck;
			OnAnchorLinked -= AnchorLinkedCallback;
			OnAnchorUnlinked -= AnchorUnlinkedCallback;
		}
	}
}
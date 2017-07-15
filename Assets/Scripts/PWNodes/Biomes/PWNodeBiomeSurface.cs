﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurface : PWNode {
	
		[PWInput, PWNotRequired]
		[PWMultiple(1, typeof(BiomeSurfaceMaps))]
		public PWValues			inputSurfaces = new PWValues();

		[PWOutput]
		public BiomeSurfaces	surfaces = new BiomeSurfaces();

		ReorderableList			layerList;
		List< ReorderableList >	slopeLists = new List< ReorderableList >();
		//complex have all maps

		int						inputIndex;

		public override void OnNodeCreate()
		{
			externalName = "Biome surface";
			layerList = new ReorderableList(surfaces.biomeLayers, typeof(BiomeSurfaceLayer));

			layerList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4;

			layerList.drawElementCallback = (rect, index, isActive, isFocused) => {
                rect.y += 2;

				var elem = surfaces.biomeLayers[index];
				EditorGUIUtility.labelWidth = 25;
				int		floatFieldSize = 70;
				int		colorFieldSize = 20;
				int		nameFieldSize = (int)rect.width - colorFieldSize - 2;
				float	lineHeight = EditorGUIUtility.singleLineHeight;
				Rect	nameRect = new Rect(rect.x, rect.y, nameFieldSize, EditorGUIUtility.singleLineHeight);
				Rect	minRect = new Rect(rect.x, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
            	Rect	maxRect = new Rect(rect.x + floatFieldSize, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
				
				elem.name = EditorGUI.TextField(nameRect, elem.name);
				elem.minHeight = EditorGUI.FloatField(minRect, "min", elem.minHeight);
				elem.maxHeight = EditorGUI.FloatField(maxRect, "max", elem.maxHeight);
			};

			layerList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "Biome layers");
			};
		}

		void	AddNewSlopeList(int index)
		{
			var layer = surfaces.biomeLayers[index];
			ReorderableList r = new ReorderableList(layer.slopeMaps, typeof(BiomeSurfaceSlopeMaps));

			r.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4;
			
			r.drawElementCallback = (rect, i, isActive, isFocused) => {
				var elem = layer.slopeMaps[i];
				
				EditorGUIUtility.labelWidth = 25;
				int		floatFieldSize = 70;
				int		colorFieldSize = 20;
				int		nameFieldSize = (int)rect.width - colorFieldSize - 2;
				float	lineHeight = EditorGUIUtility.singleLineHeight;
				Rect	nameRect = new Rect(rect.x, rect.y, nameFieldSize, EditorGUIUtility.singleLineHeight);
				Rect	minRect = new Rect(rect.x, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
            	Rect	maxRect = new Rect(rect.x + floatFieldSize, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
				
				if (elem.surfaceMaps != null && !string.IsNullOrEmpty(elem.surfaceMaps.name))
					EditorGUI.LabelField(nameRect, elem.surfaceMaps.name);
				else
					EditorGUI.LabelField(nameRect, "unnamed");
				elem.minSlope = EditorGUI.FloatField(minRect, "min", elem.minSlope);
				elem.maxSlope = EditorGUI.FloatField(maxRect, "max", elem.maxSlope);
				
				if (Event.current.type == EventType.Repaint)
					elem.y = rect.y;

				UpdatePropPosition("inputSurfaces", elem.y, i);
				UpdatePropVisibility("inputSurfaces", PWVisibility.Visible, i);
				elem.surfaceMaps = inputSurfaces.At(inputIndex) as BiomeSurfaceMaps;
				inputIndex++;
			};

			r.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "surfaces per slope");
			};
			slopeLists.Add(r);
		}

		public override void OnNodeGUI()
		{
			//Min and Max here start from the biome height min/max if there is a switch on height
			//else, min and max refer to mapped terrain value in ToBiomeData / WaterLevel node
			layerList.DoLayoutList();

			//list per slopes:
			int i = 0;
			int	slopeCount = 0;
			foreach (var layer in surfaces.biomeLayers)
				slopeCount += layer.slopeMaps.Count;
			UpdateMultiProp("inputSurfaces", slopeCount);
			foreach (var layer in surfaces.biomeLayers)
			{
				if ((layer.foldout = EditorGUILayout.Foldout(layer.foldout, layer.name)))
				{
					if (i >= slopeLists.Count)
						AddNewSlopeList(i);
					inputIndex = 0;
					slopeLists[i].DoLayoutList();
				}
				else
					UpdatePropVisibility("inputSurfaces", PWVisibility.Invisible, i);
				i++;
			}
		}

		public override void OnNodeProcess()
		{
		}
	}
}
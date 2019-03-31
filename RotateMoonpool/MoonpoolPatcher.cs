namespace RotateMoonpool
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Harmony;
    using UnityEngine;

    class MoonpoolPatcher
    {
        [HarmonyPatch(typeof(Base))]
        [HarmonyPatch("BuildMoonpoolGeometry")]
        // Figuring this out step by step is gonna be *FUN*
        internal class GeometryPatcher
        {
            [HarmonyPrefix] // We're attempting to replace the geometry building method
            public static bool Prefix(ref Base __instance, Int3 cell)
            {
                Int3 v = Base.CellSize[7];
                Int3.Bounds bounds = new Int3.Bounds(cell, cell + v - 1);
                BaseDeconstructable parent = null;
                if (__instance.GetCellMask(cell))
                {
                    Transform transform = __instance.SpawnPiece(Base.Piece.Moonpool, cell);
                    parent = BaseDeconstructable.MakeCellDeconstructable(transform, bounds, TechType.BaseMoonpool);
                    transform.tag = "MainPieceGeometry";
                }
                for (int i = 0; i < Base.moonpoolFaces.Length; i++)
                {
                    Base.RoomFace roomFace = Base.moonpoolFaces[i];
                    Base.Face face = new Base.Face(cell + roomFace.offset, roomFace.direction);
                    if (__instance.GetFaceMask(face))
                    {
                        Base.FaceType face2 = __instance.GetFace(face);
                        Base.Piece moonpoolPiece = __instance.GetMoonpoolPiece(face, face2);
                        if (moonpoolPiece != Base.Piece.Invalid)
                        {
                            Transform transform2 = __instance.SpawnPiece(moonpoolPiece, cell, roomFace.rotation, null);
                            transform2.localPosition = Int3.Scale(roomFace.offset, Base.cellSize) + roomFace.localOffset;
                            if (face2 != Base.FaceType.Solid)
                            {
                                TechType recipe = Base.FaceToRecipe[(int)face2];
                                BaseDeconstructable baseDeconstructable = BaseDeconstructable.MakeFaceDeconstructable(transform2, bounds, face, face2, recipe);
                                if (!__instance.isGhost)
                                {
                                    transform2.GetComponentsInChildren<IBaseModuleGeometry>(true, Base.sBaseModulesGeometry);
                                    int j = 0;
                                    int count = Base.sBaseModulesGeometry.Count;
                                    while (j < count)
                                    {
                                        IBaseModuleGeometry baseModuleGeometry = Base.sBaseModulesGeometry[j];
                                        baseModuleGeometry.geometryFace = face;
                                        j++;
                                    }
                                    Base.sBaseModulesGeometry.Clear();
                                    if (face2 == Base.FaceType.UpgradeConsole)
                                    {
                                        baseDeconstructable.LinkModule(new Base.Face?(new Base.Face(face.cell - __instance.anchor, face.direction)));
                                    }
                                }
                            }
                            else if (!__instance.isGhost)
                            {
                                BaseExplicitFace.MakeFaceDeconstructable(transform2, face, parent);
                            }
                        }
                    }
                }
                return false;
            }
        }
        // Holy nested brackets, batman
    }
}

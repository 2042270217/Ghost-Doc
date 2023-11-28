using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SmoothNormalToColor :Editor
{
    [MenuItem("Tools/SmoothNormalToColor")]
    static void SmoothNormalToColorFunc()
    {
        var trans = Selection.activeTransform;
        //��ȡMesh
        Mesh mesh = new Mesh();
        if (trans.GetComponent<SkinnedMeshRenderer>())
        {
            mesh = trans.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
        if (trans.GetComponent<MeshFilter>())
        {
            mesh = trans.GetComponent<MeshFilter>().sharedMesh;
        }
        Debug.Log(mesh.name);
        string NewMeshPath = "Assets/Models/"+mesh.name+"_sN.asset";
        //����һ��Vector3���飬������mesh.normalsһ�������ڴ��
        //��mesh.vertices�ж���һһ��Ӧ�Ĺ⻬�����ķ���ֵ
        Vector3[] smoothedNormals = new Vector3[mesh.normals.Length];
        //Dictionary<Vector3, List<int>> vertexDic = new Dictionary<Vector3, List<int>>();
        //for (int i = 0; i < mesh.vertices.Length; i++)
        //{
        //    if (!vertexDic.ContainsKey(mesh.vertices[i]))
        //    {
        //        List<int> vertexIndexs = new List<int>();
        //        vertexIndexs.Add(i);
        //        vertexDic.Add(mesh.vertices[i], vertexIndexs);
        //    }
        //    else
        //    {
        //        vertexDic[mesh.vertices[i]].Add(i);
        //    }
        //}
        ////ƽ����ÿ������
        //foreach (var item in vertexDic)
        //{
        //    Vector3 smoothedNormal = new Vector3(0, 0, 0);
        //    foreach (var index in item.Value)
        //    {
        //        smoothedNormal += mesh.normals[index];
        //    }
        //    //��һ��
        //    smoothedNormal.Normalize();
        //    foreach (var index in item.Value)
        //    {
        //        smoothedNormals[index] = smoothedNormal;
        //    }
        //}
        for (int i = 0; i < smoothedNormals.Length; i++)
        {
            //����һ����ֵ����
            Vector3 smoothedNormal = new Vector3(0, 0, 0);
            //����mesh.vertices���飬�����������ֵ�뵱ǰ��Ŷ���ֵ��ͬ�������Ӧ�ķ�����Normal���
            for (int j = 0; j < smoothedNormals.Length; j++)
            {
                if (mesh.vertices[j] == mesh.vertices[i])
                {
                    smoothedNormal += mesh.normals[j];
                }
            }
            //��һ��Normal����meshNormals���ж�Ӧλ�ø�ֵΪNormal,�������Ϊi�Ķ���Ķ�Ӧ���߹⻬�������
            //��ʱ��õķ���Ϊģ�Ϳռ��µķ���
            smoothedNormal.Normalize();
            smoothedNormals[i] = smoothedNormal;
        }

        //����ģ�Ϳռ�����߿ռ��ת������
        ArrayList OtoTMatrixs = new ArrayList();
        for (int i = 0; i < mesh.normals.Length; i++)
        {
            Vector3[] OtoTMatrix = new Vector3[3];
            OtoTMatrix[0] = new Vector3(mesh.tangents[i].x, mesh.tangents[i].y, mesh.tangents[i].z);
            OtoTMatrix[1] = Vector3.Cross(mesh.normals[i], OtoTMatrix[0]) * mesh.tangents[i].w;
            OtoTMatrix[2] = mesh.normals[i];
            OtoTMatrixs.Add(OtoTMatrix);
        }
        //��meshNormals�����еķ���ֵһһ�������ˣ�������߿ռ��µķ���ֵ
        for (int i = 0; i < smoothedNormals.Length; i++)
        {
            Vector3 normalTS;
            normalTS = Vector3.zero;
            normalTS.x = Vector3.Dot(((Vector3[])OtoTMatrixs[i])[0], smoothedNormals[i]);
            normalTS.y = Vector3.Dot(((Vector3[])OtoTMatrixs[i])[1], smoothedNormals[i]);
            normalTS.z = Vector3.Dot(((Vector3[])OtoTMatrixs[i])[2], smoothedNormals[i]);
            smoothedNormals[i] = normalTS;
        }

        //�½�һ����ɫ����ѹ⻬�����ķ���ֵ��������
        Color[] meshColors = new Color[mesh.colors.Length];
        for (int i = 0; i < meshColors.Length; i++)
        {
            meshColors[i].r = smoothedNormals[i].x * 0.5f + 0.5f;
            meshColors[i].g = smoothedNormals[i].y * 0.5f + 0.5f;
            meshColors[i].b = smoothedNormals[i].z * 0.5f + 0.5f;
            meshColors[i].a = mesh.colors[i].a;
        }
        //Debug.Log(mesh.colors.Length);
        //for (int i = 0; i < meshColors.Length; i++)
        //{
        //    Debug.Log(meshColors[i]);
        //}
        //return;
        //�½�һ��mesh����֮ǰmesh��������Ϣcopy��ȥ
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.normals = mesh.normals;
        newMesh.tangents = mesh.tangents;
        newMesh.uv = mesh.uv;
        newMesh.uv2 = mesh.uv2;
        newMesh.uv3 = mesh.uv3;
        newMesh.uv4 = mesh.uv4;
        newMesh.uv5 = mesh.uv5;
        newMesh.uv6 = mesh.uv6;
        newMesh.uv7 = mesh.uv7;
        newMesh.uv8 = mesh.uv8;
        //����ģ�͵���ɫ��ֵΪ����õ���ɫ
        newMesh.colors = meshColors;
        newMesh.bounds = mesh.bounds;
        newMesh.indexFormat = mesh.indexFormat;
        newMesh.bindposes = mesh.bindposes;
        newMesh.boneWeights = mesh.boneWeights;
        
        //����mesh����Ϊ.asset�ļ�                          
        AssetDatabase.CreateAsset(newMesh, NewMeshPath);
        AssetDatabase.SaveAssets();
        Debug.Log("Done");



    }
}

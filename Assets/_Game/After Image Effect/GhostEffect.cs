using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Afterimage effects, support Mesh and SkinedMesh and their combination
public class GhostEffect : MonoBehaviour
{
    //If you want Mesh to have afterimages, please ensure that the Read/Write of Mesh is turned on
    public bool IncludeMeshFilter = true;

    //Afterimage
    public class GhostImange
    {
        public Mesh mesh;
        public Material material;
        public Matrix4x4 matrix;
        public float duration;
        public float time;
    }

    //Material
    public Material EffectMaterial;

    //Total duration
    public float Duration = 5;

    //The interval of the afterimage
    public float Interval = 0.2f;

    //The afterimage fades out time
    public float FadeoutTime = 1;

    private float mTime = 5;
    private List<GhostImange> mImageList = new List<GhostImange>();

    void Start()
    {
    }

    [ContextMenu("Play")]
    public void Play()
    {
        mTime = Duration;
        StartCoroutine(AddImage());
    }

    IEnumerator AddImage()
    {
        while (mTime > 0)
        {
            CreateImage();
            yield return new WaitForSeconds(Interval);
            mTime -= Interval;
        }
    }

    void CreateImage()
    {
        SkinnedMeshRenderer[] skinRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        MeshFilter[] filters = null;

        int filtersCount = 0;

        if (IncludeMeshFilter)
        {
            filters = GetComponentsInChildren<MeshFilter>();
            filtersCount = filters.Length;
        }

        if (skinRenderers.Length + filtersCount <= 0)
        {
            return;
        }

        CombineInstance[] combineInstances = new CombineInstance[skinRenderers.Length + filtersCount];

        int idx = 0;
        for (int i = 0; i < skinRenderers.Length; i++)
        {
            var render = skinRenderers[i];

            var mesh = new Mesh();
            render.BakeMesh(mesh);

            combineInstances[idx] = new CombineInstance
            {
                mesh = mesh,
                transform = render.gameObject.transform.localToWorldMatrix,
                subMeshIndex = 0
            };

            idx++;
        }

        for (int i = 0; i < filtersCount; i++)
        {
            var render = filters[i];

            var temp = (null != render.sharedMesh) ? render.sharedMesh : render.mesh;
            var mesh = (Mesh)Object.Instantiate(temp);
            combineInstances[idx] = new CombineInstance
            {
                mesh = mesh,
                transform = render.gameObject.transform.localToWorldMatrix,
                subMeshIndex = 0
            };

            idx++;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances, true, true);

        mImageList.Add(new GhostImange
        {
            mesh = combinedMesh,
            material = new Material(EffectMaterial),
            time = FadeoutTime,
            duration = FadeoutTime,
        });
    }

    void LateUpdate()
    {
        Play();
        bool needRemove = false;


        foreach (var image in mImageList)
        {
            image.time -= Time.deltaTime;
            if (image.material.HasProperty("_Color"))
            {
                Color color = Color.white;
                color.a = Mathf.Max(0, image.time / image.duration);
                image.material.SetColor("_Color", color);
            }

            Graphics.DrawMesh(image.mesh, Matrix4x4.identity, image.material, gameObject.layer);
            if (image.time <= 0)
            {
                needRemove = true;
            }
        }

        if (needRemove)
        {
            mImageList.RemoveAll(x => x.time <= 0);
        }
    }


}
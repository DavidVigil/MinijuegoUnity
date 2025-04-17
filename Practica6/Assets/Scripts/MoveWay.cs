using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWay : MonoBehaviour
{
    public WayCreator[] pathFollow;
    public int currentWayPointID;
    public float rotSpeed;
    public float speed;
    
    public float reachDistance=0.1f;

    public int way=0;

    Vector3 last_position;
    Vector3 current_position;

    //Variables de control para los toques y tambaleo
    private int toqueCount = 0;
    private bool cayendo = false;
    private float anguloTambaleo = 10f;
    private float tiempoTambaleo = 0.2f;
    private Coroutine tambaleoActivo;
    //Variable del material para controlar el DissolveStrength
    private Material material;
    //Sistema de la explosión
    public ParticleSystem explosionFX;


    // Start is called before the first frame update
    void Start()
    {
        last_position=transform.position;
        GetComponentInChildren<ParticleSystem>().Play();
        material = GetComponentInChildren<Renderer>().material;
        material.SetFloat("_DissolveStrength", 0f); // inicial
    }

    // Update is called once per frame
    void Update()
    {
        if (cayendo) return; // ¡Esto es importante!

        float distance=Vector3.Distance(pathFollow[way].path_objs[currentWayPointID].position,transform.position);
        transform.position=Vector3.MoveTowards (transform.position,pathFollow[way].path_objs[currentWayPointID].position,Time.deltaTime*speed);
        var Rotation=Quaternion.LookRotation(pathFollow[way].path_objs[currentWayPointID].position-transform.position);
        transform.rotation=Quaternion.Slerp(transform.rotation,Rotation,Time.deltaTime*rotSpeed);


        if(distance<=reachDistance)
        {
            currentWayPointID++;
        }

        if(currentWayPointID>=pathFollow[way].path_objs.Count)
        {
            currentWayPointID= Random.Range(0, pathFollow[1].path_objs.Count);
        }
    }
    void OnMouseDown()
    {
        if (cayendo) return;

        toqueCount++;
        speed += 15.0f;
        if (way == 0)
        {
            way = 1;
            currentWayPointID = Random.Range(0, pathFollow[1].path_objs.Count);
        }
        else if (way == 1)
        {
            way = 0;
            currentWayPointID = Random.Range(0, pathFollow[0].path_objs.Count);
        }
        // Si ya hay un tambaleo corriendo, detenerlo para no solaparlo
        if (tambaleoActivo != null)
        {
            StopCoroutine(tambaleoActivo);
        }

        tambaleoActivo = StartCoroutine(Tambalear());

        if (toqueCount >= 3)
        {
            StartCoroutine(CaerAvion());
        }
    }
    IEnumerator Tambalear()
    {
        Quaternion rotInicial = transform.rotation;
        Quaternion rotIzquierda = rotInicial * Quaternion.Euler(0, 0, anguloTambaleo);
        Quaternion rotDerecha = rotInicial * Quaternion.Euler(0, 0, -anguloTambaleo);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / tiempoTambaleo;
            transform.rotation = Quaternion.Lerp(rotInicial, rotIzquierda, t);
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / tiempoTambaleo;
            transform.rotation = Quaternion.Lerp(rotIzquierda, rotDerecha, t);
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / tiempoTambaleo;
            transform.rotation = Quaternion.Lerp(rotDerecha, rotInicial, t);
            yield return null;
        }
    }
    IEnumerator CaerAvion()
    {
        cayendo = true;

        float alturaInicial = transform.position.y;
        float velocidadCaida = 2f;

        while (transform.position.y > 0f)
        {
            // Movimiento hacia abajo
            transform.position += Vector3.down * velocidadCaida * Time.deltaTime;

            // Rotación tipo descontrol
            transform.Rotate(Vector3.forward * 100f * Time.deltaTime);

            // Cálculo de disolución basado en altura
            float alturaActual = transform.position.y;
            float dissolve = Mathf.InverseLerp(alturaInicial, 0f, alturaActual);
            material.SetFloat("_DissolveStrength", dissolve-0.2f);// Ajustar el valor para que no desaparezca antes de llegar al suelo
            float burnValue = Mathf.Lerp(3f, 0f, dissolve+0.35f); // Cambia el valor de quemado según la disolución
            material.SetFloat("_BurnColor", burnValue);
            yield return null;
        }

        // Asegurar que termine completamente disuelto
        material.SetFloat("_DissolveStrength", 1f);
        //Debug.Log("Avión desintegrado al llegar al suelo");
        
        if (explosionFX != null)
        {
            explosionFX.transform.position = transform.position;
            explosionFX.Play();
        }

    }
}

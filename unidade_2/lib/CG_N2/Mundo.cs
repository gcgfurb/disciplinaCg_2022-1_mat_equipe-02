﻿/**
  Autor: Dalton Solano dos Reis
**/

#define CG_Gizmo
// #define CG_Privado

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK.Input;
using CG_Biblioteca;

namespace CG_N2
{
  public class Mundo : GameWindow
  {
    private static Mundo instanciaMundo = null;

    private IDictionary<Key, Action> customKeys = new Dictionary<Key, Action>();

    private Mundo(int width, int height, ObjetoGeometria obj) : base(width, height) {
      this.obj = obj;
     }

    private Mundo(int width, int height) : base(width, height) {
     }

    public static Mundo GetInstance(int width, int height, ObjetoGeometria obj)
    {
      if (instanciaMundo == null)
        instanciaMundo = new Mundo(width, height, obj);
      return instanciaMundo;
    }

    private CameraOrtho camera = new CameraOrtho();
    private ObjetoGeometria obj;
    protected List<Objeto> objetosLista = new List<Objeto>();
    private ObjetoGeometria objetoSelecionado = null;
    private char objetoId = '@';
    private bool bBoxDesenhar = false;

    private float[] backgroundColors = new float[4];
    private bool backgroundAlterado;
    int mouseX, mouseY;   //TODO: achar método MouseDown para não ter variável Global
    private bool mouseMoverPto = false;
#if CG_Privado
    private Privado_SegReta obj_SegReta;
    private Privado_Circulo obj_Circulo;
#endif

public void setCameraPosition(double xMin, double yMin, double xMax, double yMax)
{
  camera.xmin = xMin; camera.xmax = xMax; camera.ymin = yMin; camera.ymax = yMax;
}

public void addCustomKey(Key key, Action callback)
{
  this.customKeys.Add(key, callback);
}

public void zoomIn() {
  camera.ZoomIn();
}

public void zoomOut() {
  camera.ZoomOut();
}

public void panBaixo() {
  camera.PanBaixo();
}

public void panCima() {
  camera.PanCima();
}

public void panDireita() {
  camera.PanDireita();
}

public void panEsquerda() {
  camera.PanEsquerda();
}

public void setBackgroundColor(float red, float green, float blue, float alpha) {
      backgroundColors[0] = red;
      backgroundColors[1] = green;
      backgroundColors[2] = blue;
      backgroundColors[3] = alpha;
      backgroundAlterado = true;
}
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);  

      Console.WriteLine(" --- Ajuda / Teclas: ");
      Console.WriteLine(" [  H     ] mostra teclas usadas. ");
      if(obj != null) {
        objetosLista.Add(obj);
        objetoSelecionado = obj;
      }

#if CG_Privado
      objetoId = Utilitario.charProximo(objetoId);
      obj_SegReta = new Privado_SegReta(objetoId, null, new Ponto4D(50, 150), new Ponto4D(150, 250));
      obj_SegReta.ObjetoCor.CorR = 255; obj_SegReta.ObjetoCor.CorG = 255; obj_SegReta.ObjetoCor.CorB = 0;
      objetosLista.Add(obj_SegReta);
      objetoSelecionado = obj_SegReta;

      objetoId = Utilitario.charProximo(objetoId);
      obj_Circulo = new Privado_Circulo(objetoId, null, new Ponto4D(100, 300), 50);
      obj_Circulo.ObjetoCor.CorR = 0; obj_Circulo.ObjetoCor.CorG = 255; obj_Circulo.ObjetoCor.CorB = 255;
      objetosLista.Add(obj_Circulo);
      objetoSelecionado = obj_Circulo;
#endif
      GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadIdentity();
      GL.Ortho(camera.xmin, camera.xmax, camera.ymin, camera.ymax, camera.zmin, camera.zmax);
    }
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadIdentity();
#if CG_Gizmo      
      Sru3D();
#endif
      for (var i = 0; i < objetosLista.Count; i++)
        objetosLista[i].Desenhar();
      if (bBoxDesenhar && (objetoSelecionado != null))
        objetoSelecionado.BBox.Desenhar();
      this.SwapBuffers();
    }

    protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
    {
      if (e.Key == Key.H)
        Utilitario.AjudaTeclado();
      else if (e.Key == Key.Escape)
        Exit();

      else if (customKeys.Count > 0) {
        foreach(KeyValuePair<Key, Action> kvp in customKeys) {
          if(e.Key == kvp.Key)
              kvp.Value();
          }
      }
      else if (e.Key == Key.V)
        mouseMoverPto = !mouseMoverPto;   //TODO: falta atualizar a BBox do objeto
      else
        Console.WriteLine(" __ Tecla não implementada.");
    }

    //TODO: não está considerando o NDC
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
      mouseX = e.Position.X; mouseY = 600 - e.Position.Y; // Inverti eixo Y
      if (mouseMoverPto && (objetoSelecionado != null))
      {
        objetoSelecionado.PontosUltimo().X = mouseX;
        objetoSelecionado.PontosUltimo().Y = mouseY;
      }
    }

#if CG_Gizmo
    private void Sru3D()
    {
      GL.LineWidth(1);
      if (backgroundAlterado) {
        GL.ClearColor(backgroundColors[0], backgroundColors[1], backgroundColors[2], backgroundColors[3]);
        GL.Clear(ClearBufferMask.ColorBufferBit);
      }
      GL.Begin(PrimitiveType.Lines);
      // GL.Color3(1.0f,0.0f,0.0f);
      GL.Color3(Convert.ToByte(255), Convert.ToByte(0), Convert.ToByte(0));
      GL.Vertex3(0, 0, 0); GL.Vertex3(200, 0, 0);
      // GL.Color3(0.0f,1.0f,0.0f);
      GL.Color3(Convert.ToByte(0), Convert.ToByte(255), Convert.ToByte(0));
      GL.Vertex3(0, 0, 0); GL.Vertex3(0, 200, 0);
      // GL.Color3(0.0f,0.0f,1.0f);
      GL.Color3(Convert.ToByte(0), Convert.ToByte(0), Convert.ToByte(255));
      GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 200);
      GL.End();
    }
#endif    
  }
}

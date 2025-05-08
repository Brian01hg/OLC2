'use client';
import { Editor } from '@monaco-editor/react';
import { useState } from 'react';

const API_URL = 'http://localhost:5120';

export default function Home() {
  const [code, setCode] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [output, setOutput] = useState<string>('');

  const handleExecute = async () => {
    try {
      const response = await fetch(`${API_URL}/compile`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ code }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error || 'Error desconocido');
      }

      setOutput(data.result);
      setError('');
    } catch (err) {
      setOutput('');
      setError(err instanceof Error ? err.message : 'Error desconocido');
    }
  };

  const openReporteErrores = () => {
    window.open('http://localhost:5120/errores', '_blank');
  };
  
  const openReporteSimbolos = () => {
    window.open('http://localhost:5120/simbolos', '_blank');
  };

  const handleCopyOutput = () => {
    if (output) {
      navigator.clipboard.writeText(output)
        .then(() => alert('Resultado copiado al portapapeles'))
        .catch((err) => alert('Error al copiar al portapapeles: ' + err));
    }
  };

  return (
    <div className='flex flex-col items-center justify-center min-h-screen py-2'>
      <Editor
        height='70vh'
        defaultLanguage='javascript'
        theme='vs-dark'
        value={code}
        onChange={(value) => setCode(value || '')}
      />

      <div className="flex gap-4 mt-4">
        <button
          className='bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded'
          onClick={handleExecute}
        >
          Ejecutar
        </button>

        <button
          className='bg-green-600 hover:bg-green-800 text-white font-bold py-2 px-4 rounded'
          onClick={openReporteErrores}
        >
          Ver errores
        </button>

        <button
          className='bg-purple-600 hover:bg-purple-800 text-white font-bold py-2 px-4 rounded'
          onClick={openReporteSimbolos}
        >
          Ver tabla de símbolos
        </button>

        <button
          className='bg-yellow-500 hover:bg-yellow-700 text-white font-bold py-2 px-4 rounded'
          onClick={handleCopyOutput}
        >
          Copiar resultado
        </button>
      </div>

      {output && (
        <div className='flex flex-col items-center justify-center mt-6'>
          <h2 className="text-lg font-semibold">Output:</h2>
          <pre className="bg-gray-800 text-white p-4 rounded max-w-4xl overflow-auto">{output}</pre>
        </div>
      )}
      {error && (
        <div className='flex flex-col items-center justify-center bg-red-500 text-white p-4 mt-6 rounded'>
          <h2>Error:</h2>
          <pre>{error}</pre>
        </div>
      )}
    </div>
  );
}

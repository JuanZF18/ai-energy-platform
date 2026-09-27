import { initializeApp } from 'firebase/app'
import { getAuth, onAuthStateChanged, signInWithEmailAndPassword, signOut } from 'firebase/auth'
import type { Auth, User } from 'firebase/auth'
import type { FirebaseWebConfig } from '@/lib/types'

const signInErrors: Record<string, string> = {
  'auth/invalid-credential': 'Correo o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.',
  'auth/invalid-email': 'Correo o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.',
  'auth/user-disabled': 'Esta cuenta está desactivada. Pide acceso al administrador.',
  'auth/too-many-requests': 'Demasiados intentos seguidos. Espera un momento e inténtalo de nuevo.',
  'auth/network-request-failed': 'No hay conexión con el servicio de inicio de sesión. Revisa tu internet.',
  'auth/operation-not-allowed': 'El inicio de sesión con correo no está activado en Firebase.',
  'auth/configuration-not-found': 'Firebase Authentication no está activado en el proyecto. Actívalo en la consola de Firebase.',
  'auth/unauthorized-domain': 'Este dominio no está autorizado en Firebase Authentication.',
  'auth/api-key-not-valid.-please-pass-a-valid-api-key.': 'La configuración de Firebase no es válida. Revisa la clave web del proyecto.',
}

export interface FirebaseSession {
  auth: Auth
  signIn: (email: string, password: string) => Promise<void>
  signOut: () => Promise<void>
  token: () => Promise<string | null>
  watch: (listener: (user: User | null) => void) => () => void
}

export function connectFirebase(config: FirebaseWebConfig): FirebaseSession {
  const auth = getAuth(initializeApp(config))

  return {
    auth,
    signIn: async (email, password) => {
      try {
        await signInWithEmailAndPassword(auth, email.trim(), password)
      } catch (error) {
        const code = (error as { code?: string }).code ?? ''
        if (!signInErrors[code]) console.error('Firebase rechazó el inicio de sesión', code)
        throw new Error(signInErrors[code] ?? 'No pudimos iniciar sesión. Inténtalo de nuevo.')
      }
    },
    signOut: () => signOut(auth),
    token: async () => (auth.currentUser ? auth.currentUser.getIdToken() : null),
    watch: (listener) => onAuthStateChanged(auth, listener),
  }
}

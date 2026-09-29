import AppNavigation from '../components/navigation/AppNavigation'

export default function AppLayout({ children }) {
  return (
    <div className="app-layout">
      <AppNavigation />
      <main className="app-content">{children}</main>
    </div>
  )
}
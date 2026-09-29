import { formatJoinedDate } from '../../utils/dateFormatter'

export default function ProfileMeta({ createdAt }) {
  const joined = formatJoinedDate(createdAt)

  if (!joined) return null

  return <p className="profile-meta">{joined}</p>
}
import { apiRequest } from './api'

export const socialService = {
  getFriends() {
    return apiRequest('/api/social/friends')
  },

  getReceivedRequests() {
    return apiRequest('/api/social/requests/received')
  },

  getSentRequests() {
    return apiRequest('/api/social/requests/sent')
  },

  getStatus(targetId) {
    return apiRequest(`/api/social/status/${targetId}`)
  },

  sendRequest(targetId) {
    return apiRequest(`/api/social/requests/${targetId}`, { method: 'POST' })
  },

  acceptRequest(requesterId) {
    return apiRequest(`/api/social/requests/${requesterId}/accept`, { method: 'POST' })
  },

  rejectRequest(requesterId) {
    return apiRequest(`/api/social/requests/${requesterId}/reject`, { method: 'POST' })
  },

  cancelRequest(addresseeId) {
    return apiRequest(`/api/social/requests/${addresseeId}`, { method: 'DELETE' })
  },

  removeFriend(friendId) {
    return apiRequest(`/api/social/friends/${friendId}`, { method: 'DELETE' })
  },

  follow(targetId) {
    return apiRequest(`/api/social/follow/${targetId}`, { method: 'POST' })
  },

  unfollow(targetId) {
    return apiRequest(`/api/social/follow/${targetId}`, { method: 'DELETE' })
  },

  block(targetId) {
    return apiRequest(`/api/social/block/${targetId}`, { method: 'POST' })
  },

  unblock(targetId) {
    return apiRequest(`/api/social/block/${targetId}`, { method: 'DELETE' })
  },

  /** Los perfiles que el usuario bloqueó, para poder desbloquearlos. */
  getBlocks() {
    return apiRequest('/api/social/blocks')
  },
}
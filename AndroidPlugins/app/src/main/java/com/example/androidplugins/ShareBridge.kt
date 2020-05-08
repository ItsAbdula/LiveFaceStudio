package com.example.androidplugins

import java.text.SimpleDateFormat
import java.util.*

import android.app.Activity
import android.content.Intent
import android.widget.Toast
import com.unity3d.player.UnityPlayer


fun Date.toString(format: String, locale: Locale = Locale.getDefault()): String {
    val formatter = SimpleDateFormat(format, locale)
    return formatter.format(this)
}

fun getCurrentDateTime(): Date {
    return Calendar.getInstance().time
}

class ShareBridge constructor(_unityActivity: Activity) {
    private val unityActivity: Activity = _unityActivity

    fun sendToUnity(message: String) {
        UnityPlayer.UnitySendMessage("AndroidManager", "ReceiveFromAndroid", message)
    }

    fun sayHelloToUnity(){
        val date = getCurrentDateTime()
        val dateInString = date.toString("HH:mm:ss")
        sendToUnity("[$dateInString]Hello, Unity!")
    }

    fun makeToast(message: String) {
        Toast.makeText(unityActivity, message, Toast.LENGTH_SHORT).show()
    }

    fun shareText(message: String) {
        val shareIntent: Intent = Intent().apply {
            action = Intent.ACTION_SEND
            putExtra(Intent.EXTRA_TEXT, message)
            type = "text/plain"
        }

        unityActivity.startActivity(Intent.createChooser(shareIntent, "Share using"))
    }

    fun shareImage(shareType: String, uriToImage: String) {
        val shareIntent: Intent = Intent().apply {
            action = Intent.ACTION_SEND
            putExtra(Intent.EXTRA_STREAM, uriToImage)
            type = shareType
        }

        unityActivity.startActivity(Intent.createChooser(shareIntent, "Share using"))
    }
}